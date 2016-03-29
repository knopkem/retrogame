using System;
using System.Collections.Generic;
using GameEngine.DebugTools;
using GameEngine.ScreenManager;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RetroGame.Screens
{
    /// <summary>
    /// This class should be used for game screens that need access to game data, e.g. use characters
    /// By default it draws a HUD, but this can be deactivated (UseHud)
    /// </summary>
    internal class ExtendedGameScreen : MenuScreen
    {
        protected List<Character2D> ActiveMembers;
        protected CharPool CharPoolObj;
        protected ContentManager Content;
        protected Camera2D GameCamera;
        protected SpriteFont GameFont;
        protected HudDisplay Hud;
        protected Viewport HudViewport;
        protected InputHandler InputHandler;
        protected MapHelper MapHelper;
        protected Viewport MapViewport;
        protected SharedData SharedDataObj;
        protected TimeSpan StartTime;

        protected float PauseAlpha;
        protected bool ResetTime;
        protected float Scale = 1;
        

        public ExtendedGameScreen() : base("")
        {
            // create new
            InputHandler = new InputHandler();
            UseHud = true;
        }

        public bool UseHud { get; set; }

        public override void LoadContent()
        {
           
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            GameFont = Content.Load<SpriteFont>("SpriteFonts/gamefont");


            //-----DEBUG ONLY------

            // check if the game is initialized
            if (!GameManager.Instance.IsInitialized)
            {
                TraceLog.Write("Initializing game-manager with default values for debugging.");
                GameManager.Instance.Initialize(Content);
                GameManager.Instance.StartNewGame();
                SharedDataObj = GameManager.Instance.Data;
                MapHelper = SharedDataObj.GlobalMapHelper;
                CharPoolObj = SharedDataObj.GlobalCharPool;

                // select all players
                for (int i = 0; i < CharPoolObj.GetNumberOfAvailableChars(); i++)
                {
                    GameManager.Instance.Data.GlobalCharPool.SelectChar(
                        GameManager.Instance.Data.GlobalCharPool.AvailableChars.GetChar(i));
                }
            }

            // ---DEBUG END ---

            // now loading data
            SharedDataObj = GameManager.Instance.Data;
            MapHelper = SharedDataObj.GlobalMapHelper;
            CharPoolObj = SharedDataObj.GlobalCharPool;

            // setting up the hud
            if (UseHud)
            {
                Hud = new HudDisplay(ScreenManager);
                Hud.LoadContent(Content);
                Hud.ItemSelected += HudItemSelected;

                ScreenManager.ResetViewport();
                HudViewport = ScreenManager.GraphicsDevice.Viewport;
                HudViewport.Height = Hud.HudSize.Y;
                HudViewport.Y = 0;

                MapViewport = ScreenManager.GraphicsDevice.Viewport;
                MapViewport.Height = MapViewport.Height - Hud.HudSize.Y;
                MapViewport.Y = MapViewport.Y + Hud.HudSize.Y;
            }

            // Initialize camera using the boundaries defined in the map
            GameCamera = new Camera2D(MapViewport, MapHelper.GameMap);

            // Initialize camera using a custom rectangle to define the boundaries
            GameCamera = new Camera2D(ScreenManager.GraphicsDevice.Viewport,
                                      new Rectangle(1*MapHelper.GameMap.TileWidth, 1*MapHelper.GameMap.TileHeight,
                                                    MapHelper.GameMap.Width*MapHelper.GameMap.TileWidth,
                                                    MapHelper.GameMap.Height*MapHelper.GameMap.TileHeight));

            // add active members (members that are drawn)
            ActiveMembers = new List<Character2D>();
            for (int i = 0; i < CharPoolObj.SelectedChars.Count; i++)
            {
                ActiveMembers.Add(CharPoolObj.SelectedChars.GetChar(i));
            }


            // focus on first one
            if (CharPoolObj.GetCurrentCharacter() != null)
                GameCamera.FocusOnObject(CharPoolObj.GetCurrentCharacter().Cursor);
            else
                TraceLog.Write("No Object to focus on");

            base.LoadContent();

        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            //if (gameTime.IsRunningSlowly)
            //TraceLog.Write("RUNNING SLOWLY");
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Update the hud
            if (UseHud)
                Hud.Update(gameTime, input);


            // Look up inputs for the active player profile.
            if (ControllingPlayer != null)
            {
                var playerIndex = (int) ControllingPlayer.Value;
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
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime)
        {
            if (UseHud)
            {
                // Get the spritebatch from screenmanager
                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

                ScreenManager.GraphicsDevice.Viewport = HudViewport;
                // the hud needs to be drawn without the cameraMatrix
                spriteBatch.Begin(0, null, null, null, null, null, ScreenManager.ScalingMatrix);
                Hud.ViewRectangle = HudViewport.Bounds;
                Hud.Draw(spriteBatch);
                spriteBatch.End();

                ScreenManager.GraphicsDevice.Viewport = MapViewport;
            }

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || PauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, PauseAlpha/2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

            base.Draw(gameTime);
        }

        protected virtual void HudItemSelected(object sender, HudEventArgs e)
        {
            TraceLog.WriteWarning("Default implementation called!");
        }

        /// <summary>
        /// Get next Team member
        /// </summary>
        protected virtual void CycleThroughPlayers()
        {
            if (CharPoolObj.SelectedChars.Count <= 0) return;

            CharPoolObj.SelectedChars.Next();
            Character2D player = CharPoolObj.GetCurrentCharacter();
            GameCamera.FocusOnObject(player.Cursor);
        }

        /// <summary>
        /// if using the efficient drawing, we use an overload of Draw that takes in the area to
        /// draw in order to compensate for large maps that only need to draw what's on screen.
        /// our visible area is computed using the camera and the viewport size.
        /// </summary>
        public Rectangle GetVisibleArea()
        {
            var visibleArea = new Rectangle((int) GameCamera.Position.X, (int) GameCamera.Position.Y,
                                            (int) ((ScreenManager.PrefScreenSize.X + GameCamera.Position.X)*Scale),
                                            (int) ((ScreenManager.PrefScreenSize.Y + GameCamera.Position.Y)*Scale));

            return visibleArea;
        }
    }
}