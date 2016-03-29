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
    /// This class is shows a game screen where the user can hire/fire his team members
    /// Interaction is done using the base class menu structure
    /// </summary>
    internal class MemberSelectionScreen : ExtendedGameScreen
    {
        #region Fields

        private Texture2D backgroundTexture;
        private SpriteFont menuFont;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public MemberSelectionScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            UseHud = false;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            // call before!
            base.LoadContent();

            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            menuFont = Content.Load<SpriteFont>("SpriteFonts/menufont");
            backgroundTexture = Content.Load<Texture2D>("Backgrounds/MemberSelection");

            // Create our menu entries.
            var nextEntry = new MenuEntry("Next");
            var prevEntry = new MenuEntry("Previous");
            var hireEntry = new MenuEntry("Hire");
            var retrEntry = new MenuEntry("Return");

            // Hook up menu event handlers.
            nextEntry.Selected += nextEntry_Selected;
            prevEntry.Selected += prevEntry_Selected;
            hireEntry.Selected += hireEntry_Selected;
            retrEntry.Selected += retrEntry_Selected;

            // Add entries to the menu.
            MenuEntries.Add(nextEntry);
            MenuEntries.Add(prevEntry);
            MenuEntries.Add(hireEntry);
            MenuEntries.Add(retrEntry);
        }

        private void retrEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, 0,
                               new BackgroundScreen(BackgroundScreen.ContentReference.Planning),
                               new GameMainMenuScreen());
        }

        private void hireEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            CharPoolObj.SelectCurrentChar();
        }

        private void prevEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            CharPoolObj.AvailableChars.Prev();
        }

        private void nextEntry_Selected(object sender, PlayerIndexEventArgs e)
        {
            CharPoolObj.AvailableChars.Next();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // draw the background            
            spriteBatch.Begin();

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            var fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            spriteBatch.Draw(backgroundTexture, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();

            // scaled overlay
            spriteBatch.Begin(0, null, null, null, null, null, ScreenManager.ScalingMatrix);

            // draw the photo            
            Character2D player = CharPoolObj.AvailableChars.GetCurrentChar();
            var destRect = new Rectangle(150, 60, 100, 400/3);
            spriteBatch.Draw(player.SpriteTexture2D, destRect, Color.White);

            // draw description
            spriteBatch.DrawString(menuFont, player.CharProps.FullTextStringList[0],
                                   new Vector2(300, 60), Color.LimeGreen, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            // draw stats
            string stats = "Force lock: " + player.CharProps.ForceLock + "\n" + "Jamming: " + player.CharProps.Jamming +
                           "\n" +
                           "Pick lock: " + player.CharProps.LockPicking + "\n" + "Safecracking: " +
                           player.CharProps.SafeCracking + "\n" +
                           "Strength: " + player.CharProps.Strength + "\n" + "Stealth: " + player.CharProps.Stealth;
            spriteBatch.DrawString(menuFont, stats, new Vector2(150, 220), Color.LimeGreen, 0, Vector2.Zero, 1f,
                                   SpriteEffects.None, 0);

            // draw hired status
            if (player.IsSelected)
                spriteBatch.DrawString(menuFont, "Hired", new Vector2(200, 180), Color.IndianRed);


            spriteBatch.End();

            // draw the menu
            base.Draw(gameTime);
        }

        #endregion
    }
}