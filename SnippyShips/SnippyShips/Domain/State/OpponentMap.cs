using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SnippyShips.Domain.Command.Direction;
using System.Drawing;

namespace SnippyShips.Domain.State
{
    public class OpponentMap
    {
        [JsonProperty]
        public bool Alive { get; set; }
        [JsonProperty]
        public int Points { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public List<OpponentShip> Ships { get; set; }
        [JsonProperty]
        public List<OpponentCell> Cells { get; set; }

        public bool Hunting { get; set; } = false;

        public OpponentCell GetCellAt(int x, int y)
        {
            return Cells.FirstOrDefault(p => p.X == x && p.Y == y);
        }

        public OpponentCell GetAdjacentCell(OpponentCell cell, Direction direction)
        {
            if (cell == null)
                return null;

            switch (direction)
            {
                case Direction.North:
                    return GetCellAt(cell.X, cell.Y + 1);
                case Direction.South:
                    return GetCellAt(cell.X, cell.Y - 1);
                case Direction.West:
                    return GetCellAt(cell.X - 1, cell.Y);
                case Direction.East:
                    return GetCellAt(cell.X + 1, cell.Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public List<OpponentCell> GetAllCellsInDirection(Point startLocation, Direction direction, int length)
        {
            var startCell = GetCellAt(startLocation.X, startLocation.Y);
            var cells = new List<OpponentCell>() { startCell };

            if (startCell == null)
                return cells;

            for (int i = 1; i < length; i++)
            {
                var nextCell = GetAdjacentCell(startCell, direction);
                if (nextCell == null)
                    throw new ArgumentException("Not enough cells for requested length");

                cells.Add(nextCell);
                startCell = nextCell;
            }

            return cells;
        }

        public bool HasCellsForDirection(Point startLocation, Direction direction, int length)
        {
            var startCell = GetCellAt(startLocation.X, startLocation.Y);

            if (startCell == null)
                return false;

            for (int i = 1; i < length; i++)
            {
                var nextCell = GetAdjacentCell(startCell, direction);
                if (nextCell == null)
                    return false;

                startCell = nextCell;
            }

            return true;
        }

        internal void CalcProbabilities(BattleshipPlayer player)
        {
            int remainingHits = 0;
            Ships.ForEach(ship => { if( ship.Destroyed == false) remainingHits += Ship.Size(ship.ShipType); });
            if( (17 - player.ShotsHit) < remainingHits )
            {
                // We have a target that needs to be hunted
                Hunting = true;
            }

            var directions = new Direction[] { Direction.North, Direction.South, Direction.East, Direction.West };

            Cells.ForEach(cell =>
            {
                if (cell.Missed || cell.Damaged) return;

                Ships.ForEach(ship =>
                {
                    foreach (Direction dir in directions)
                    {
                        if (HasCellsForDirection(cell.Point, dir, Ship.Size(ship.ShipType)) == false) continue;
                        List<OpponentCell> cells = GetAllCellsInDirection(cell.Point, dir, Ship.Size(ship.ShipType));
                        if ((cells.Any(c => c.Damaged) == true) && Hunting)
                        {
                            foreach (OpponentCell oc in cells)
                            {
                                if (oc.Missed == true) break;
                                if (oc.Damaged == true)
                                    cell.Probability += 2;
                            }
                        }
                        if (cells.Any(c => c.Missed) == true) continue;

                        cell.Probability++;
                    }
                });
            });
        }
    }
}
