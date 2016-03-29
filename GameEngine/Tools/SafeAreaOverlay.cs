#region File Description

//-----------------------------------------------------------------------------
// SafeAreaOverlay.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace GameEngine.Tools
{
    /// <summary>
    /// Reusable component makes it easy to check whether your important
    /// graphics are positioned inside the title safe area, by superimposing
    /// a red border that marks the edges of the safe region.
    /// </summary>
    public class SafeAreaOverlay : DrawableGameComponent
    {
        private Texture2D   _dummyTexture;
        private SpriteBatch _spriteBatch;


        /// <summary>
        /// Constructor.
        /// </summary>
        public SafeAreaOverlay(Game game)
            : base(game)
        {
            // Choose a high number, so we will draw on top of other components.
            DrawOrder = 1000;
        }


        /// <summary>
        /// Creates the graphics resources needed to draw the overlay.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white texture.
            _dummyTexture = new Texture2D(GraphicsDevice, 1, 1);

            _dummyTexture.SetData(new[] {Color.White});
        }


        /// <summary>
        /// Draws the title safe area.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Look up the current viewport and safe area dimensions.
            Viewport viewport = GraphicsDevice.Viewport;

            Rectangle safeArea = viewport.TitleSafeArea;

            int viewportRight = viewport.X + viewport.Width;
            int viewportBottom = viewport.Y + viewport.Height;

            // Compute four border rectangles around the edges of the safe area.
            var leftBorder = new Rectangle(viewport.X,
                                           viewport.Y,
                                           safeArea.X - viewport.X,
                                           viewport.Height);

            var rightBorder = new Rectangle(safeArea.Right,
                                            viewport.Y,
                                            viewportRight - safeArea.Right,
                                            viewport.Height);

            var topBorder = new Rectangle(safeArea.Left,
                                          viewport.Y,
                                          safeArea.Width,
                                          safeArea.Top - viewport.Y);

            var bottomBorder = new Rectangle(safeArea.Left,
                                             safeArea.Bottom,
                                             safeArea.Width,
                                             viewportBottom - safeArea.Bottom);

            // Draw the safe area borders.
            Color translucentRed = Color.Red*0.5f;

            _spriteBatch.Begin();

            _spriteBatch.Draw(_dummyTexture, leftBorder, translucentRed);
            _spriteBatch.Draw(_dummyTexture, rightBorder, translucentRed);
            _spriteBatch.Draw(_dummyTexture, topBorder, translucentRed);
            _spriteBatch.Draw(_dummyTexture, bottomBorder, translucentRed);

            _spriteBatch.End();
        }
    }
}