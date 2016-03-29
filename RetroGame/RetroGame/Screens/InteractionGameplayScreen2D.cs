#region Using Statements

using System;
using System.Collections.Generic;
using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// This screen should play the recorded paths
    /// </summary>
    internal class InteractionGameplayScreen2D : ExtendedGameScreen
    {
        #region Fields

        private bool _firstUpdate;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public InteractionGameplayScreen2D()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // utilize drag and flick to move the camera
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Flick;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            // call before!
            base.LoadContent();

            // Execute recorded tasks
            foreach (Character2D character in ActiveMembers)
                character.ExecuteTaskList();
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
            if (IsActive)
            {
                if (!_firstUpdate)
                {
                    StartTime = gameTime.TotalGameTime;
                    _firstUpdate = true;
                }

                // Update our objects
                foreach (Character2D character in ActiveMembers)
                    character.Update(gameTime, GameCamera);

                // Update the camera                
                GameCamera.Update();
            }

            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {

            // get the current char to control
            PlayerIndex index;
            Character2D currentChar = CharPoolObj.GetCurrentCharacter();

            // switch player
            if (input.IsNewKeyPress(Keys.F2, ControllingPlayer, out index) ||
                input.IsNewButtonPress(Buttons.X, ControllingPlayer, out index))
            {
                CycleThroughPlayers();
            }

                // zoom
            else if (input.IsNewKeyPress(Keys.PageDown, ControllingPlayer, out index))
            {
                if (Scale < 5) Scale += 0.05f;
            }
            else if (input.IsNewKeyPress(Keys.PageUp, ControllingPlayer, out index))
            {
                if (Scale > 1)
                    Scale += -0.05f;
            }
            
                // show dynamic menu with available tasks
            else if (input.IsNewKeyPress(Keys.F1, ControllingPlayer, out index))
            {
                var menu = new List<string> {"Replay", "Back"};
                var dynamicMenu = new DynamicMenuScreen(menu);
                dynamicMenu.SelectedMenu += DynamicMenuSelectedMenu;
                ScreenManager.AddScreen(dynamicMenu, ControllingPlayer);
            }

            base.HandleInput(gameTime, input);
        }

        /// <summary>
        /// dynamic menu but should be shown on escape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamicMenuSelectedMenu(object sender, MenuEntry.CustomResultEventArgs e)
        {
            string selectedMenu = e.text;

            switch (selectedMenu)
            {
                case "Replay":
                    _firstUpdate = false;
                    foreach (Character2D member in ActiveMembers)
                        member.ExecuteTaskList(); // start playback
                    break;
                case "Back":
                    LoadingScreen.Load(ScreenManager, false, 0, new PlanningGameplayScreen());
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Draws the game play screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            
            var cameraMatrix = GameCamera.GetCameraMatrix(Scale);

            // we use 0 and null to indicate that we just want the default values
            spriteBatch.Begin(0, null, null, null, null, null, cameraMatrix);

            // draw the map using the engine
            MapHelper.Draw(spriteBatch, GetVisibleArea());

            // Draw the player sprite after the map including the cursor and the path
            foreach (Character2D character in ActiveMembers)
                character.Draw(spriteBatch);

            // show the elapsed GameTime
            double elapsedTime = (gameTime.TotalGameTime - StartTime).TotalSeconds;
            double roundedTime = Math.Round(elapsedTime*100)/100;
            spriteBatch.DrawString(GameFont, "TotalTime: " + roundedTime,
                                   CharPoolObj.GetCurrentCharacter().Position + new Vector2(0, 30), Color.DarkGreen);

            spriteBatch.End();

            // draw hud
            base.Draw(gameTime);
        }

        #endregion
    }
}