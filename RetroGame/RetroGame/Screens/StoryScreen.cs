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
using Microsoft.Xna.Framework.Input;
using RetroGameData;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// This is the base screen for all story telling screens. 
    /// It consists of text and images that blend with time.
    /// </summary>
    public class StoryScreen : GameScreen
    {
        #region Fields

        private readonly float scrollSpeed;
        private ContentManager content;
        private SpriteFont introFont;
        private float pauseAlpha;
        private CutSceneData sceneData;

        private Vector2 textPosition = new Vector2(0, 0);

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public StoryScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            scrollSpeed = -0.3f;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            textPosition.X = 50;
            textPosition.Y = ScreenManager.GraphicsDevice.Viewport.Height + 10;

            introFont = content.Load<SpriteFont>("SpriteFonts/introfont");
            sceneData = content.Load<CutSceneData>("CutScenes/Intro/IntroData");


            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
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
                pauseAlpha = Math.Min(pauseAlpha + 1f/32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f/32, 0);

            if (IsActive)
            {
                textPosition.Y += scrollSpeed;


                if (textPosition.Y < 0)
                {
                    PlayerIndex index = PlayerIndex.One;
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
            var playerIndex = (int) ControllingPlayer.Value;

            PlayerIndex index = PlayerIndex.One;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {
                // Skip this intro                
                if (keyboardState.IsKeyDown(Keys.Space))
                    ContinueToNextScreen(index);
            }
        }

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
            string storyText = sceneData.StoryText[0];

            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.DrawString(introFont, storyText, textPosition, Color.DarkRed);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha/2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        #endregion
    }
}