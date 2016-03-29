using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// Adds sprite animation routines to a controllableObject
    /// Not used for the moment...
    /// </summary>
    public class AnimatedObject : ControllableObject
    {
        private readonly TimeSpan   _animSpeed = TimeSpan.FromMilliseconds(100);
        private TimeSpan            _animCount = TimeSpan.FromMilliseconds(0);
        private int                 _currentFrame;
        private int                 _currentRow;
        private const int           FrameHeight = 192;
        private const int           FrameWidth = 128;
        private const int           NumFrames = 6;


        public AnimatedObject(MapHelper map, Vector2 pos, string assetName)
            : base(map, pos, assetName)
        {
            MathHelper.ToRadians(270);
        }

        public override void Update(GameTime gameTime, Camera2D gameCamera)
        {
            if (Target != Position)
            {
                // Move toward the target vector
                Vector2 moveVect = Target - Position;
                moveVect.Normalize();

                // Work out which animation row to use (which direction we're facing)
                _currentRow = GetTileRow(moveVect);

                // Do some animation
                _animCount += gameTime.ElapsedGameTime;
                if (_animCount > _animSpeed)
                {
                    _animCount = TimeSpan.FromMilliseconds(0);

                    _currentFrame += 1;
                    if (_currentFrame == NumFrames) _currentFrame = 0;
                }
            }
            base.Update(gameTime, gameCamera);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var sourceRect = new Rectangle(_currentFrame*FrameWidth, _currentRow*FrameHeight, FrameWidth, FrameHeight);

            spriteBatch.Draw(SpriteTexture2D, Position, sourceRect, Color.White, 0f, DrawOrigin, DrawScale, SpriteEffects.None, 0);
        }

        private static int GetTileRow(Vector2 moveVect)
        {
            // Convert a movement vector to face direction
            float angle = ((float) Math.Atan2(-moveVect.Y, -moveVect.X) + MathHelper.TwoPi)%MathHelper.TwoPi;
            int polarRegion = (int) Math.Round(angle*8f/MathHelper.TwoPi)%8;

            // Do a little bit of jigging because our sprite sheet isn't in order
            polarRegion += 2;
            if (polarRegion > 7) polarRegion -= 8;

            return polarRegion;
        }
    }
}