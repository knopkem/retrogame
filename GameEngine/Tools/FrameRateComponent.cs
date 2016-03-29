using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Tools
{
    /// <summary>
    /// A real basic frame rate counter.
    /// </summary>
    public class FrameRateComponent : DrawableGameComponent
    {
        private int         _drawCount;
        private string      _drawString = "FPS: ";
        private float       _drawTimer;

        private SpriteFont  _font;
        private SpriteBatch _spriteBatch;

        public FrameRateComponent(Game game)
            : base(game)
        {
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("Font");
        }

        public override void Draw(GameTime gameTime)
        {
            _drawCount++;
            _drawTimer += (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (_drawTimer >= 1f)
            {
                _drawTimer -= 1f;
                _drawString = "FPS: " + _drawCount;
                _drawCount = 0;
            }

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, _drawString, new Vector2(10f, 10f), Color.White);
            _spriteBatch.End();
        }
    }
}