using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Algorithms;
using GameEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// Calculates the shortest path between two points and draws it. 
    /// The path can be retrieved and used to control other objects
    /// </summary>
    public class PathCalculator : ControllableObject
    {
        public bool IsDrawingLine;
        public List<Point> PathWay;
        public List<Waypoint> PathWayWorld;
        
        private readonly PathFinderFast _finder;
        private readonly byte[,] _grid;
        private Point _lastStartPt;
        private Point _lastStopPt;
        private List<PathFinderNode> _resultList;


        public PathCalculator(MapHelper map, Vector2 pos, string assetName)
            : base(map, pos, assetName)
        {
            _grid = new byte[map.GameMap.Width,map.GameMap.Height];

            // convert gameMap to grid
            for (int i = 0; i < map.GameMap.Width; i++)
                for (int u = 0; u < map.GameMap.Height; u++)
                {
                    if (map.IsAreaBlocked(new Point(i, u)))
                        _grid[i, u] = 0; // open
                    else
                        _grid[i, u] = 1; // closed
                }

            // our pathfinder algorithm gets initialized here
            _finder = new PathFinderFast(_grid);
            _finder.Diagonals = true;
            _finder.HeavyDiagonals = true;
            _finder.PunishChangeDirection = false;
            _finder.TieBreaker = true;
            _finder.SearchLimit = map.GameMap.Width*map.GameMap.Height;

            PathWay = new List<Point>();
            PathWayWorld = new List<Waypoint>();
        }


        private List<PathFinderNode> GetShortestPathToTile(Point startPt, Point stopPt)
        {
            List<Point> freeTiles = MapHelperObj.GetListOfAvailableTiles(stopPt);

            if (freeTiles.Count == 0)
                return new List<PathFinderNode>();

            var paths = new List<List<PathFinderNode>>();
            foreach (Point tile in freeTiles)
            {
                List<PathFinderNode> foundPath = _finder.FindPath(startPt, tile);
                var copyList = new List<PathFinderNode>();
                if (foundPath != null)
                {
                    foreach (PathFinderNode node in foundPath)
                        copyList.Add(node);
                    paths.Add(copyList);
                }
            }

            var sortedList = new CompactFrameWorkExtensions.SortedList<SortablePath>();
            foreach (var result in paths)
            {
                sortedList.AddHead(new SortablePath(result.Count, result));
            }

            if (sortedList.Count() > 0)
                return sortedList.ElementAt(0).Path;
            else
                return new List<PathFinderNode>();
        }

        public void Update(Vector2 start, Vector2 stop)
        {
            // convert pixel pos to tiles
            Point startPt = MapHelperObj.WorldToTile(start);
            Point stopPt = MapHelperObj.WorldToTile(stop);

            // only try to find a path if we are not on a blocked field
            if (MapHelperObj.IsTilePositionAvailable(stopPt))
            {
                // get the list of available tiles

                // do path finding on all and return the shortest
                if ((_lastStartPt.Equals(startPt) && _lastStopPt.Equals(stopPt)))
                    _resultList = GetShortestPathToTile(startPt, stopPt);

                // do path finding now
                //resultList = finder.FindPath(startPt, stopPt);
            }
            else
            {
                // delete the current path
                if (_resultList != null)
                    _resultList.Clear();
            }
            _lastStartPt = startPt;
            _lastStopPt = stopPt;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            PathFinderNode lastNode;
            lastNode.X = -1;
            lastNode.Y = -1;

            if (_resultList != null)
                foreach (PathFinderNode node in _resultList)
                {
                    // don't draw it if it is just one tile
                    if (_resultList.Count == 1)
                        break;

                    if (IsDrawingLine)
                    {
                        if (lastNode.X != -1)
                        {
                            DrawLine(spriteBatch, SpriteTexture2D, MapHelperObj.TileToWorld(new Point(lastNode.X, lastNode.Y)),
                                     MapHelperObj.TileToWorld(new Point(node.X, node.Y)), Color.Red);
                        }
                    }
                    else
                    {
                        spriteBatch.Draw(SpriteTexture2D, MapHelperObj.TileToWorld(new Point(node.X, node.Y)), Color.Red);
                    }

                    lastNode = node;
                }
        }

        /// <summary>
        /// compute the path in tile position only
        /// </summary>
        /// <returns></returns>
        private List<Point> GetTilePath()
        {
            PathWay.Clear();
            if (_resultList != null)
                foreach (PathFinderNode node in _resultList)
                    PathWay.Add(new Point(node.X, node.Y));
            if (PathWay.Count == 1)
                PathWay.Clear();

            return PathWay;
        }

        /// <summary>
        /// return the path between source and target (world and tile)
        /// </summary>
        /// <returns></returns>
        public List<Waypoint> GetComputedPath()
        {
            // convert tile to world
            PathWayWorld.Clear();
            GetTilePath();
            foreach (Point point in PathWay)
                PathWayWorld.Insert(0, new Waypoint(MapHelperObj.TileToWorld(point), point));
            return PathWayWorld;
        }

        /// <summary>
        /// Use the current texture to draw a line. The texture is streched in the target direction.
        /// </summary>
        /// <param name="sprBatch"></param>
        /// <param name="spr"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="col"></param>
        private static void DrawLine(SpriteBatch sprBatch, Texture2D spr, Vector2 a, Vector2 b, Color col)
        {
            var origin = new Vector2(0.5f, 0.0f);
            Vector2 diff = b - a;
            var scale = new Vector2(1.0f, diff.Length()/spr.Height);

            float angle = (float) (Math.Atan2(diff.Y, diff.X)) - MathHelper.PiOver2;

            sprBatch.Draw(spr, a, null, col, angle, origin, scale, SpriteEffects.None, 1.0f);
        }

        #region Nested type: SortablePath

        private class SortablePath : IComparable<SortablePath>
        {
            private readonly int _length;
            private readonly List<PathFinderNode> _path;

            public SortablePath(int length, List<PathFinderNode> path)
            {
                this._length = length;
                this._path = path;
            }

            public List<PathFinderNode> Path
            {
                get { return _path; }
            }

            // This will cause list elements to be sorted on age values.

            #region IComparable<SortablePath> Members

            public int CompareTo(SortablePath p)
            {
                return _length - p._length;
            }

            #endregion

            // Must implement Equals.
            public bool Equals(SortablePath p)
            {
                return (_length == p._length);
            }
        }

        #endregion
    }
}