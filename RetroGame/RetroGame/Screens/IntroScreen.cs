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
using System.Text;
using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using RetroGameData;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// Present an introduction to the user 
    /// </summary>
    internal class IntroScreen : ExtendedGameScreen
    {
        #region Fields

        private readonly float _scrollSpeed;
        private string _storyText;
        private SpriteFont _introFont;
        private Vector2 _textPosition = new Vector2(0, 0);
        private CutSceneData _cutData;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public IntroScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            _scrollSpeed = -0.2f;

            EnabledGestures = GestureType.Tap;

        }



        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            _introFont = Content.Load<SpriteFont>("SpriteFonts/menufont");

            // load the cutscene data from xml
            _cutData = Content.Load<CutSceneData>("CutScenes/Intro/IntroData");

            // convert to string
            var builder = new StringBuilder();
            foreach (string line in _cutData.StoryText)
                builder.Append(line);
            _storyText = builder.ToString();

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            _textPosition.X = 50;

            _textPosition.Y = ScreenManager.PrefScreenSize.Y;
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
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                PauseAlpha = Math.Min(PauseAlpha + 1f/32, 1);
            else
                PauseAlpha = Math.Max(PauseAlpha - 1f/32, 0);

            if (IsActive)
            {
                _textPosition.Y += _scrollSpeed;


                if (_textPosition.Y < 0)
                {
                    const PlayerIndex index = PlayerIndex.One;
                    ContinueToNextScreen(index);
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            if (ControllingPlayer != null)
            {
                var playerIndex = (int) ControllingPlayer.Value;

                KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
                GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

                // The game pauses either if the user presses the pause button, or if
                // they unplug the active gamepad. This requires us to keep track of
                // whether a gamepad was ever plugged in, because we don't want to pause
                // on PC if they are playing with a keyboard and have no gamepad at all!
                bool gamePadDisconnected = !gamePadState.IsConnected && input.GamePadWasConnected[playerIndex];

                if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
                {
                    ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
                }
                else
                {
                    // looks for xbox and windows default keys
                    PlayerIndex index = PlayerIndex.One;
                    if (input.IsMenuSelect(ControllingPlayer, out index))
                        ContinueToNextScreen(index);

                    // look for any taps that occurred and select any entries that were tapped
                    foreach (GestureSample gesture in input.Gestures)
                    {
                        if (gesture.GestureType == GestureType.Tap)
                            ContinueToNextScreen(index);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the next screen
        /// </summary>
        /// <param name="playerIndex"></param>
        private void ContinueToNextScreen(PlayerIndex playerIndex)
        {
            LoadingScreen.Load(ScreenManager, false, playerIndex,
                               new BackgroundScreen(BackgroundScreen.ContentReference.Planning),
                               new GameMainMenuScreen());
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // scaled text
            spriteBatch.Begin(0, null, null, null, null, null, ScreenManager.ScalingMatrix);

            spriteBatch.DrawString(_introFont, _storyText, _textPosition, Color.DarkRed, 0, Vector2.Zero, 0.7f,
                                   SpriteEffects.None, 0);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || PauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, PauseAlpha/2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        #endregion
    }
}