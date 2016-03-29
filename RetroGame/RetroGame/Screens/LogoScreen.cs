#region File Description

//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// This is the screen that fades in the logo and switches to the next
    /// </summary>
    internal class LogoScreen : GameScreen
    {
        #region Fields

        private ContentManager  _content;
        private SpriteFont      _gameFont;
        private readonly float  _pauseAlpha;
        private Vector2         _textPosition = new Vector2(0, 0);
        private int             _timespan;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public LogoScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            _pauseAlpha = 0.0f;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (_content == null)
                _content = new ContentManager(ScreenManager.Game.Services, "Content");

            _gameFont = _content.Load<SpriteFont>("SpriteFonts/gamefont");

            _timespan = 0;
            _textPosition.X = ScreenManager.GraphicsDevice.Viewport.Width/2 - 100;
            _textPosition.Y = ScreenManager.GraphicsDevice.Viewport.Height/2 - 30;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            //content.Unload();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            if (_timespan == 0)
            {
                _timespan = gameTime.TotalGameTime.Seconds;
            }

            if ((gameTime.TotalGameTime.Seconds - _timespan) > 0.5)
            {
                _timespan = 0;
                const PlayerIndex index = PlayerIndex.One;
                ContinueToNextScreen(index);
            }

            base.Update(gameTime, otherScreenHasFocus, false);
        }


        private void ContinueToNextScreen(PlayerIndex playerIndex)
        {
            LoadingScreen.Load(ScreenManager, false, playerIndex,
                               new BackgroundScreen(BackgroundScreen.ContentReference.MainBackground),
                               new MainMenuScreen());
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.DrawString(_gameFont, "Retro-K-Bros", _textPosition, Color.DarkRed, 0, Vector2.Zero, 2f,
                                   SpriteEffects.None, 0);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || _pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, _pauseAlpha/2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        #endregion
    }
}