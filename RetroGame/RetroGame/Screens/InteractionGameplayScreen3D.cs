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
using GameEngine.Geometry;
using GameEngine.ScreenManager;
using GameEngine.TileEngine;
using GameEngine.WorldGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace RetroGame.Screens
{
    /// <summary>
    /// This screen implements the interaction phase using 3d models and walls  
    /// </summary>
    internal class InteractionGameplayScreen3D : ExtendedGameScreen
    {
        #region Fields

        protected List<Character3D>     ActiveMembers3D;
        
        private BaseCamera              _baseCamera;
        private int                     _currentPlayer;
        private WorldGenerator          _worldGen;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public InteractionGameplayScreen3D()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // utilize drag and flick to move the camera
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Flick | GestureType.Tap;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            // needs to be called before
            base.LoadContent();

            // Initialize camera using a custom rectangle to define the boundaries
            GameCamera = new Camera2D(ScreenManager.GraphicsDevice.Viewport,
                                      new Rectangle(MapHelper.GameMap.TileWidth, MapHelper.GameMap.TileHeight,
                                                    MapHelper.GameMap.Width * MapHelper.GameMap.TileWidth,
                                                    MapHelper.GameMap.Height * MapHelper.GameMap.TileHeight));

            // init the 3d camera
            _baseCamera = new BaseCamera(ScreenManager.Game) {ViewPort = ScreenManager.GraphicsDevice.Viewport};
            _baseCamera.Initialize();


            // init the world generator
            _worldGen = new WorldGenerator(ScreenManager.Game, ScreenManager.GraphicsDevice, MapHelper.GameMap);
            _worldGen.SetCamera(_baseCamera);

            // init to player 1
            ActiveMembers3D = new List<Character3D>();
            for (int i = 0; i < CharPoolObj.GetNumberOfSelectedChars(); i++)
            {
                var member = new Character3D(CharPoolObj.SelectedChars.GetChar(i));
                member.LoadContent(Content);
                ActiveMembers3D.Add(member);
                _baseCamera.FocusOnObject(GetCurrentCharacter());
                member.ExecuteTaskList(); // start playback
            }
        }

        protected override void HudItemSelected(object sender, HudEventArgs e)
        {
            if (e.ButtonType == 1)
                CycleThroughPlayers();
            else
                LoadingScreen.Load(ScreenManager, false, 0, new PlanningGameplayScreen());
        }

        /// <summary>
        /// Get next Team-member
        /// </summary>
        protected override void CycleThroughPlayers()
        {
            CharPoolObj.SelectedChars.Next();
            NextChar();
            _baseCamera.FocusOnObject(GetCurrentCharacter());
        }

        private void NextChar()
        {
            if (_currentPlayer < ActiveMembers3D.Count - 1)
                _currentPlayer++;
            else
                _currentPlayer = 0;
        }

        private Character3D GetCurrentCharacter()
        {
            return ActiveMembers3D[_currentPlayer];
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
                _worldGen.Update(gameTime);

                // Draw the player sprite after the map including the cursor and the path
                foreach (Character3D character in ActiveMembers3D)
                {
                    character.ViewMatrix = _worldGen.ViewMatrix;
                    character.ProjectionMatrix = _worldGen.ProjectionMatrix;
                    character.Update(gameTime, GameCamera);
                }
            }

            // class base
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {

            PlayerIndex index;

            // switch player
            if (input.IsNewKeyPress(Keys.F2, ControllingPlayer, out index) ||
                input.IsNewButtonPress(Buttons.X, ControllingPlayer, out index))
            {
                CycleThroughPlayers();
            }

                // dynamic menu
            else if (input.IsNewKeyPress(Keys.F3, ControllingPlayer, out index) ||
                     input.IsNewButtonPress(Buttons.Y, ControllingPlayer, out index))
            {
                LoadingScreen.Load(ScreenManager, false, 0, new PlanningGameplayScreen());
            }

            base.HandleInput(gameTime, input);
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // draw the world
            _worldGen.Draw();

            // draw all members
            foreach (Character3D character in ActiveMembers3D)
                character.Draw();

            // draw base
            base.Draw(gameTime);
        }

        #endregion
    }
}