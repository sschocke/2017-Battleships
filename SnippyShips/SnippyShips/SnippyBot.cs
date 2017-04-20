using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SnippyShips.Domain.Command;
using SnippyShips.Domain.Command.Code;
using SnippyShips.Domain.Command.Direction;
using SnippyShips.Domain.Command.Ship;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using SnippyShips.Domain.State;

namespace SnippyShips
{
    public class SnippyBot
    {
        protected string WorkingDirectory { get; set; }
        protected string Key { get; set; }

        private const string CommandFileName = "command.txt";

        private const string PlaceShipFileName = "place.txt";

        private const string StateFileName = "state.json";

        public SnippyBot(string key, string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            Key = key;
        }

        public void Execute()
        {
            GameState state = JsonConvert.DeserializeObject<GameState>(LoadState());

            int phase = state.Phase;

            if (phase == 1)
            {
                var placeShips = PlaceShips(state);
                WritePlaceShips(placeShips);
            }
            else
            {
                var move = MakeMove(state);
                WriteMove(move);
            }
        }

        private PlaceShipCommand PlaceShips(GameState state)
        {
            Random rnd = new Random();

            var shipsToPlace = new List<ShipType>
            {
                ShipType.Battleship,
                ShipType.Carrier,
                ShipType.Cruiser,
                ShipType.Destroyer,
                ShipType.Submarine
            };

            List<ShipType> ships = new List<ShipType>();
            List<Point> coordinates = new List<Point>();
            List<Direction> directions = new List<Direction>();

            while( shipsToPlace.Any())
            {
                var ship = shipsToPlace.First();

                Point coord = new Point(rnd.Next(0, state.PlayerMap.MapWidth - 1), rnd.Next(0, state.PlayerMap.MapHeight - 1));
                var availDirections = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                availDirections = availDirections.OrderBy(x => rnd.Next()).ToArray();
                foreach (Direction dir in availDirections)
                {
                    // Check if it can fit
                    if (state.PlayerMap.HasCellsForDirection(coord, dir, Ship.Size(ship)) == false) continue;
                    List<Cell> cells = state.PlayerMap.GetAllCellsInDirection(coord, dir, Ship.Size(ship));
                    if (cells.Any(cell => cell.Occupied)) continue;

                    // If it does, add to the commands, mark cells as occupied and remove from list
                    ships.Add(ship);
                    coordinates.Add(coord);
                    directions.Add(dir);

                    cells.ForEach(cell => { cell.Occupied = true; });

                    shipsToPlace.Remove(ship);
                    break;
                }
            }

            return new PlaceShipCommand
            {
                Ships = ships,
                Coordinates = coordinates,
                Directions = directions
            };
        }

        private Command MakeMove(GameState state)
        {
            state.OpponentMap.CalcProbabilities(state.PlayerMap.Owner);
            var sortedCells = state.OpponentMap.Cells.OrderByDescending(cell => cell.Probability);
            int bestProbability = sortedCells.First().Probability;
            var bestCells = from OpponentCell oc in sortedCells
                            where oc.Probability == bestProbability
                            select oc;

            Random rnd = new Random();
            var targetCell = bestCells.ToArray()[rnd.Next(bestCells.Count())];

            var heatmapFile = Path.Combine(WorkingDirectory, "heatmap.png");
            Bitmap heatmap = new Bitmap(state.PlayerMap.MapWidth * 32, (state.PlayerMap.MapHeight * 32) + 100);
            Graphics heatpage = Graphics.FromImage(heatmap);
            Font gridfont = new Font(FontFamily.GenericMonospace, 22, GraphicsUnit.Pixel);
            state.OpponentMap.Cells.ForEach(cell =>
            {
            int heatlevel = 255 - Math.Min(cell.Probability * 10, 255);
            Color heat = Color.FromArgb(heatlevel, heatlevel, heatlevel);
            Brush heatbrush = new SolidBrush(heat);
            heatpage.FillRectangle(heatbrush, cell.X * 32, cell.Y * 32, 31, 31);
            heatpage.DrawRectangle(Pens.Black, cell.X * 32, cell.Y * 32, 31, 31);
            if (cell == targetCell)
                heatpage.DrawRectangle(Pens.Red, cell.X * 32, cell.Y * 32, 31, 31);
            if (cell.Missed)
            {
                heatpage.DrawString("X", gridfont, Brushes.Black, (cell.X * 32) + 5, (cell.Y * 32) + 5);
            }
            if (cell.Damaged)
            {
                //heatpage.DrawString("O", gridfont, Brushes.Red, (cell.X * 32) + 5, (cell.Y * 32) + 5);
                heatpage.FillEllipse(Brushes.Red, (cell.X * 32) + 4, (cell.Y * 32) + 4, 24, 24);
                }
            });
            int yOffset = (state.PlayerMap.MapHeight * 32) + 10;
            Font font = new Font(FontFamily.GenericMonospace, 8, GraphicsUnit.Pixel);
            heatpage.DrawString((state.OpponentMap.Hunting ? "Destroy" : "Hunting"), font, Brushes.White, 5, yOffset);
            for( int s=0; s<state.OpponentMap.Ships.Count; s++)
            {
                var ship = state.OpponentMap.Ships[s];
                if (ship.Destroyed == false) {
                    heatpage.DrawString(ship.ShipType.ToString(), font, Brushes.White, 5, yOffset + (s * 10) + 10);
                }
            }
            heatmap.Save(heatmapFile);


            //var random = new Random();
            var code = Code.FireShot;
            return new Command(code, targetCell.X, targetCell.Y);
        }

        private string LoadState()
        {
            var filename = Path.Combine(WorkingDirectory, StateFileName);
            try
            {
                string jsonText;
                using (var file = new StreamReader(filename))
                {
                    jsonText = file.ReadToEnd();
                }

                return jsonText;
            }
            catch (IOException e)
            {
                Log($"Unable to read state file: {filename}");
                var trace = new StackTrace(e);
                Log($"Stacktrace: {trace}");
                return null;
            }
        }

        private void WriteMove(Command command)
        {
            var filename = Path.Combine(WorkingDirectory, CommandFileName);

            try
            {
                using (var file = new StreamWriter(filename))
                {
                    file.WriteLine(command);
                }

                Log("Command: " + command);
            }
            catch (IOException e)
            {
                Log($"Unable to write command file: {filename}");

                var trace = new StackTrace(e);
                Log($"Stacktrace: {trace}");
            }
        }

        private void WritePlaceShips(PlaceShipCommand placeShipCommand)
        {
            var filename = Path.Combine(WorkingDirectory, PlaceShipFileName);
            try
            {
                using (var file = new StreamWriter(filename))
                {
                    file.WriteLine(placeShipCommand);
                }

                Log("Placeship command: " + placeShipCommand);
            }
            catch (IOException e)
            {
                Log($"Unable to write place ship command file: {filename}");

                var trace = new StackTrace(e);
                Log($"Stacktrace: {trace}");
            }

        }

        private void Log(string message)
        {
            Console.WriteLine("[BOT]\t{0}", message);
        }
    }
}
