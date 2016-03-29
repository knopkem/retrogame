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
using System.Collections.Generic;
using GameEngine.DebugTools;
using GameEngine.ScreenManager;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// This screen represents the planning phase. It allows moving the team on a floorplan.
    /// </summary>
    internal class PlanningGameplayScreen : ExtendedGameScreen
    {
        #region Fields

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlanningGameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // utilize drag and flick to move the camera
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DoubleTap | GestureType.Tap;
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
                character.StopExecutingTaskList();
        }

        /// <summary>
        /// The hud send events back to this class in order to let the game react on user input
        /// </summary>
        /// <param name="sender">the hud</param>
        /// <param name="e">Used to distinct between different button types</param>
        protected override void HudItemSelected(object sender, HudEventArgs e)
        {
            if (e.ButtonType == 1)
                CycleThroughPlayers();
            else
                LoadingScreen.Load(ScreenManager, false, 0, new InteractionGameplayScreen3D());
        }

        #endregion

        /// <summary>
        /// Assign the selected task to the current player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamicMenuSelectedMenu(object sender, MenuEntry.CustomResultEventArgs e)
        {
            string taskName = e.text;
            Character2D player = CharPoolObj.GetCurrentCharacter();
            ETasks newTask = CommonHelper.NameToTaskE(taskName);
            player.AddTask(newTask);

            // add new properties to the object under the cursor at the estimated time for this task
            MapObjectExtended mapObj = SharedDataObj.GlobalMapObjectManager.GetObjectAtPosition(player.Cursor.Position);
            CommonHelper.ProcessObject(newTask, player.GetTotalTimeOfTaskList(), ref mapObj);
        }

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
                // Update all of our selected members
                foreach (Character2D character in CharPoolObj.SelectedChars)
                    character.Update(gameTime, GameCamera);

                // Update the camera                
                GameCamera.Update();
            }

            if (ResetTime)
            {
                StartTime = gameTime.TotalGameTime;
                ResetTime = false;
            }

            base.Update(gameTime, otherScreenHasFocus, false);

        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the game play screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // Look up inputs for the active player profile.
            if (ControllingPlayer != null)
            {
                var playerIndex = (int) ControllingPlayer.Value;

                // get the current char to control
                PlayerIndex index;
                Character2D currentChar = CharPoolObj.GetCurrentCharacter();

                // cast it due to the reference
                ControllableObject obj = currentChar.Cursor;
            
                // now update our cursor
                InputHandler.TakeControlOfObject(input, playerIndex, ref obj);
            
                if (input.IsMenuSelect(ControllingPlayer, out index))
                {
                    MapObjectExtended mapObj =
                        SharedDataObj.GlobalMapObjectManager.GetObjectAtPosition(currentChar.Cursor.Position);
                    // get the object at specific current time + walk path
                    List<string> menu = CommonHelper.GetAvailableTasksAsStringList(mapObj,
                                                                                   currentChar.GetTotalTimeOfTaskList() +
                                                                                   currentChar.GetTimeOfActivePath());
                    var dynamicMenu = new DynamicMenuScreen(menu);
                    dynamicMenu.SelectedMenu += DynamicMenuSelectedMenu;
                    ScreenManager.AddScreen(dynamicMenu, ControllingPlayer);
                }
                
                    // switch player
                else if (input.IsNewKeyPress(Keys.F2, ControllingPlayer, out index) ||
                         input.IsNewButtonPress(Buttons.X, ControllingPlayer, out index))
                {
                    this.CycleThroughPlayers();
                }
                
                    // execute playback in 2D
                else if (input.IsNewKeyPress(Keys.F3, ControllingPlayer, out index) ||
                         input.IsNewButtonPress(Buttons.Y, ControllingPlayer, out index))
                {
                    LoadingScreen.Load(ScreenManager, false, 0, new InteractionGameplayScreen2D());
                }
            
                    // execute playback in 3D (experimental)
                else if (input.IsNewKeyPress(Keys.F4, ControllingPlayer, out index))
                {
                    LoadingScreen.Load(ScreenManager, false, 0, new InteractionGameplayScreen3D());
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
            }


            base.HandleInput(gameTime, input);
        }


        /// <summary>
        /// Draws the game play screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);

            // Initialize
            Character2D player = CharPoolObj.GetCurrentCharacter();
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // get the matrix from the current camera position
            var cameraMatrix = GameCamera.GetCameraMatrix(Scale);

            // we use 0 and null to indicate that we just want the default values
            spriteBatch.Begin(0, null, null, null, null, null, cameraMatrix);

            // draw the map using the engine
            MapHelper.Draw(spriteBatch, GetVisibleArea());

            // Draw the player after the map including the cursor and the path
            CharPoolObj.GetCurrentCharacter().Draw(spriteBatch);

            spriteBatch.End();


            // let's use the base class to draw the hud
            base.Draw(gameTime);
        }

        #endregion
    }
}