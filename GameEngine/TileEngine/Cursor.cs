using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// The cursor moves only from tile to tile
    /// </summary>
    public class Cursor : ControllableObject
    {
        public Cursor(MapHelper map, Vector2 pos, string assetName)
            : base(map, pos, assetName)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // snap to tile
            Vector2 snapPosition = MapHelperObj.WorldSnapToTile(Position);

            spriteBatch.Draw(SpriteTexture2D, snapPosition, null, Color.White, 0f, DrawOrigin, DrawScale, SpriteEffects.None, 0);
        }
    }
}