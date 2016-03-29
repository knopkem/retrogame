using Microsoft.Xna.Framework;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// Class that holds data about a tileposition
    /// </summary>
    public class Waypoint
    {
        public bool IsStoredWaypoint;
        public Vector2 Position;
        public Point TileIndex;

        public Waypoint(Vector2 pos, Point tile)
        {
            Position = pos;
            TileIndex = tile;
        }
    }
}