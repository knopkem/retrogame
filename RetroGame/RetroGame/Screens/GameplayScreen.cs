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
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using GameEngine.ScreenManager;
using GameEngine.TileEngine;
using TiledLib;

#endregion

namespace RetroGame
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;
    
        Map map;
        Camera gameCamera;
        AnimatedObject gamePlayer;
         // position of the camera
        Vector2 camera = Vector2.Zero;

        // the speed of camera movement; used on phone to let us "flick" the camera
        Vector2 cameraVelocity = Vector2.Zero;

        float moveSpeed = 3f;
        private int pauseAlpha = 0;
        
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
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
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("SpriteFonts/gamefont");

            // load the map
            map = content.Load<Map>("TestMap");

            /*
            gamePlayer = new AnimatedObject(new MapHeler(mapHelper, new Vector2(80, 80), "wizard");
            gamePlayer.LoadContent(content);
            gamePlayer.DrawOrigin = new Vector2(64, 160);
           */
           
            // Initialise camera using a custom rectangle to define the boundaries
            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, new Rectangle(1 * map.TileWidth, 1 * map.TileHeight, map.Width * map.TileWidth, map.Height * map.TileHeight));


        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
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

            if (IsActive)
            {
                /*
                // Update the player
                gamePlayer.Update(gameTime, map, gameCamera);

                // Set camera target to player location (minus half the screen width and height to center on the player)
                gameCamera.Target = gamePlayer.Position - new Vector2(gameCamera.Width / 2, gameCamera.Height / 2);

                
                // Update the camera
                gameCamera.Update();
                */
            }

        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

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
                /*

               #if WINDOWS_PHONE
                           // if we have a finger on the screen, set the velocity to 0
                           if (TouchPanel.GetState().Count > 0)
                           {
                               cameraVelocity = Vector2.Zero;
                           }

                           // update our camera with the velocity
                           MoveCamera(cameraVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds);

                           // apply some friction to the camera velocity
                           cameraVelocity *= 1f - (.95f * (float)gameTime.ElapsedGameTime.TotalSeconds);

                           while (TouchPanel.IsGestureAvailable)
                           {
                               GestureSample gesture = TouchPanel.ReadGesture();

                               // just move the camera if we have a drag
                               if (gesture.GestureType == GestureType.FreeDrag)
                               {
                                   MoveCamera(-gesture.Delta);
                               }

                               // set our velocity if we see a flick
                               else if (gesture.GestureType == GestureType.Flick)
                               {
                                   cameraVelocity = -gesture.Delta;
                               }
                           }
               #else
                               // update the camera
                               UpdateCamera(gamePadState, keyboardState);
               #endif

                           }
                       }


                       private void UpdateCamera(GamePadState gamePadState, KeyboardState keyboardState)
                       {
                           Vector2 cameraMovement = Vector2.Zero;

                           // if a gamepad is connected, use the left thumbstick to move the camera
                           if (gamePadState.IsConnected)
                           {
                               cameraMovement.X = gamePadState.ThumbSticks.Left.X;
                               cameraMovement.Y = -gamePadState.ThumbSticks.Left.Y;
                           }
                           else
                           {
                               // otherwise we use the arrow keys
                               if (keyboardState.IsKeyDown(Keys.Left))
                                   cameraMovement.X = -1;
                               else if (keyboardState.IsKeyDown(Keys.Right))
                                   cameraMovement.X = 1;
                               if (keyboardState.IsKeyDown(Keys.Up))
                                   cameraMovement.Y = -1;
                               else if (keyboardState.IsKeyDown(Keys.Down))
                                   cameraMovement.Y = 1;

                               // to match the thumbstick behavior, we need to normalize non-zero vectors in case the user
                               // is pressing a diagonal direction.
                               if (cameraMovement != Vector2.Zero)
                                   cameraMovement.Normalize();
                           }

                           // scale our movement to move 25 pixels per second
                           cameraMovement *= 10f;

                           // move the camera
                           MoveCamera(cameraMovement);
                 */

                // XBOX
                if (gamePadState.ThumbSticks.Left.Length() > 0.2f)
                {
                    gamePlayer.Target = (gamePlayer.Position + ((gamePadState.ThumbSticks.Left * new Vector2(1, -1f)) * 50.0f));
                }
                else
                {
                    Vector2 moveVect;
                    moveVect = new Vector2(0, 0);
 
                    // WINDOWS
                    if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        moveVect += new Vector2(-moveSpeed, 0);
                    }
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        moveVect += new Vector2(moveSpeed, 0);
                    }
                    if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        moveVect += new Vector2(0, -moveSpeed);
                    }
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        moveVect += new Vector2(0, moveSpeed);
                    }

                    if ((moveVect.X != 0) || (moveVect.Y != 0))
                    {
                        moveVect.Normalize();
                        moveVect.X *= moveSpeed;
                        moveVect.Y *= moveSpeed;
                    }

                    gamePlayer.Target = gamePlayer.Position + moveVect;
                }

            }
             
        }

        /*
        private void MoveCamera(Vector2 cameraMovement)
        {
            camera += cameraMovement;

            // clamp the camera so it never leaves the visible area of the map. we
            Vector2 cameraMax = new Vector2(
                map.Width * map.TileWidth - ScreenManager.GraphicsDevice.Viewport.Width,
                map.Height * map.TileHeight - ScreenManager.GraphicsDevice.Viewport.Height);
            camera = Vector2.Clamp(camera, Vector2.Zero, cameraMax);
        }
        */


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            float scale = 2;

            // create a matrix for the camera to offset everything we draw, the map and our objects. since the
            // camera coordinates are where the camera is, we offset everything by the negative of that to simulate
            // a camera moving. we also cast to integers to avoid filtering artifacts
            Matrix cameraMatrix = Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0);
            cameraMatrix *= Matrix.CreateScale(1/scale);

            // we use 0 and null to indicate that we just want the default values
            spriteBatch.Begin(0, null, null, null, null, null, cameraMatrix);

          
            // if using the efficient drawing, we use an overload of Draw that takes in the area to
            // draw in order to compensate for large maps that only need to draw what's on screen.
            // our visible area is computed using the camera and the viewport size.
            Rectangle visibleArea = new Rectangle(
                                        (int)gameCamera.Position.X,
                                        (int)gameCamera.Position.Y,
                                        (int)(ScreenManager.GraphicsDevice.Viewport.Width * scale) + (int)gameCamera.Position.X,
                                        (int)(ScreenManager.GraphicsDevice.Viewport.Height * scale) + (int)gameCamera.Position.Y);
            map.Draw(spriteBatch, visibleArea);

            // Draw the player sprite after the map
            //gamePlayer.Draw(spriteBatch, gameCamera);

          
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
