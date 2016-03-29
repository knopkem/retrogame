using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledLib;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// HelperClass to provide common methods needed with TiledLib
    /// </summary>
    public class MapHelper
    {
        private readonly string     _mapAssetName;
        private MapObjectLayer      _objectLayerObjects;
        private TileLayer           _tileLayerCollision;
        private TileLayer           _tileLayerWalls;

        public MapHelper(string assetName)
        {
            _mapAssetName = assetName;
        }

        public Map GameMap { get; set; }

        public void LoadContent(ContentManager content)
        {
            GameMap = content.Load<Map>(_mapAssetName);
            _tileLayerWalls = GameMap.GetLayer("Walls") as TileLayer;
            _tileLayerCollision = GameMap.GetLayer("Collision") as TileLayer;
            _objectLayerObjects = GameMap.GetLayer("Objects") as MapObjectLayer;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            GameMap.Draw(spriteBatch, visibleArea);
        }


        /// <summary>
        /// Convert PixelPosition to TileIndex
        /// </summary>
        /// <returns>Index in TileArray</returns>
        public Point WorldToTile(Vector2 worldPoint)
        {
            //if (worldPoint.Y < GameMap.TileHeight * 
            return GameMap.WorldPointToTileIndex(worldPoint);
        }


        /// <summary>
        /// Convert TileIndex to PixelPosition
        /// </summary>
        /// <param name="tilePt"></param>
        /// <returns></returns>
        public Vector2 TileToWorld(Point tilePt)
        {
            var pos = new Vector2(0, 0)
                          {
                              X = (float) ((tilePt.X*GameMap.TileWidth) + (0.5*GameMap.TileWidth)),
                              Y = (float) ((tilePt.Y*GameMap.TileHeight) + (0.5*GameMap.TileHeight))
                          };
            return pos;
        }

        /// <summary>
        /// Converts a world to tile position and back having the effect to center the position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2 WorldSnapToTile(Vector2 position)
        {
            return TileToWorld(WorldToTile(position));
        }

        /// <summary>
        /// Return a list of objects found in this map
        /// </summary>
        /// <param name="tileIndex"></param>
        /// <returns></returns>
        public ReadOnlyCollection<MapObject> GetObjects()
        {
            return _objectLayerObjects.Objects;
        }

        /// <summary>
        /// Return true if tileIndex is not defined in map
        /// </summary>
        /// <param name="tileIndex"></param>
        /// <returns></returns>
        public bool IsOutOfTileBounds(Point tileIndex)
        {
            bool isNotDefined = false;
            if ((tileIndex.X > GameMap.Width - 1) || (tileIndex.Y > GameMap.Height - 1))
                isNotDefined = true;
            if ((tileIndex.X < 0) || (tileIndex.Y < 0))
                isNotDefined = true;

            return isNotDefined;
        }

        /// <summary>
        /// Returns true if the cursor can be reached from any side
        /// Only the 4 front sides are 
        /// </summary>
        public bool IsTilePositionAvailable(Point tile)
        {
            // check all tiles around the cursor
            if (IsAreaBlocked(tile))
            {
                if (IsAreaBlocked(new Point(tile.X, tile.Y - 3)))
                {
                    if (IsAreaBlocked(new Point(tile.X, tile.Y + 3)))
                    {
                        if (IsAreaBlocked(new Point(tile.X - 3, tile.Y)))
                        {
                            if (IsAreaBlocked(new Point(tile.X + 3, tile.Y)))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns all tiles that can be reached
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public List<Point> GetListOfAvailableTiles(Point tile)
        {
            int offset = 1;
            var list = new List<Point>();

            // if the tile is free we return only this 
            if (!IsAreaBlocked(tile))
                list.Add(tile);
            else
            {
                var bottom = new Point(tile.X, tile.Y - offset);
                if (!IsAreaBlocked(bottom))
                    list.Add(bottom);

                var top = new Point(tile.X, tile.Y + offset);
                if (!IsAreaBlocked(top))
                    list.Add(top);

                var left = new Point(tile.X - offset, tile.Y);
                if (!IsAreaBlocked(left))
                    list.Add(left);

                var right = new Point(tile.X + offset, tile.Y);
                if (!IsAreaBlocked(right))
                    list.Add(right);

                // increase range and check again
                if (list.Count == 0)
                {
                    offset = 2;
                    bottom = new Point(tile.X, tile.Y - offset);
                    if (!IsAreaBlocked(bottom))
                        list.Add(bottom);

                    top = new Point(tile.X, tile.Y + offset);
                    if (!IsAreaBlocked(top))
                        list.Add(top);

                    left = new Point(tile.X - offset, tile.Y);
                    if (!IsAreaBlocked(left))
                        list.Add(left);

                    right = new Point(tile.X + offset, tile.Y);
                    if (!IsAreaBlocked(right))
                        list.Add(right);
                }
            }
            return list;
        }

        /// <summary>
        /// Check whether a pixel position contains to a tile that is blocked
        /// </summary>
        /// <param name="pixelPos"></param>
        /// <returns></returns>
        public bool IsBlocked(Vector2 pixelPos)
        {
            return IsBlocked(WorldToTile(pixelPos));
        }

        /// <summary>
        /// Check whether a Tile is marked as blocked or not
        /// </summary>
        /// <param name="tileIndex"></param>
        /// <returns></returns>
        public bool IsBlocked(Point tileIndex)
        {
            bool isBlocked = false;

            if (!IsOutOfTileBounds(tileIndex))
            {
                Tile tile = _tileLayerWalls.Tiles[tileIndex.X, tileIndex.Y];
                if (tile != null)
                    isBlocked = true;
                tile = _tileLayerCollision.Tiles[tileIndex.X, tileIndex.Y];
                if (tile != null)
                    isBlocked = true;
            }

            return isBlocked;
        }

        /// <summary>
        /// Check if pixel position or one of its neighboring pixel/tiles are blocked
        /// </summary>
        /// <param name="pixelPos"></param>
        /// <returns></returns>
        public bool IsAreaBlocked(Vector2 pixelPos)
        {
            return IsAreaBlocked(WorldToTile(pixelPos));
        }

        /// <summary>
        /// Check if the tileIndex or one of its neighbors is blocked
        /// </summary>
        /// <param name="tileIndex"></param>
        /// <returns></returns>
        public bool IsAreaBlocked(Point tileIndex)
        {
            bool isBlocked = false;
            if (IsBlocked(new Point(tileIndex.X - 1, tileIndex.Y - 1)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X - 1, tileIndex.Y)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X, tileIndex.Y - 1)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X, tileIndex.Y)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X + 1, tileIndex.Y)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X, tileIndex.Y + 1)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X + 1, tileIndex.Y + 1)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X + 1, tileIndex.Y - 1)))
                isBlocked = true;
            if (IsBlocked(new Point(tileIndex.X - 1, tileIndex.Y + 1)))
                isBlocked = true;


            return isBlocked;
        }
    }
}