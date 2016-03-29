#define IsDebug

using System;
using System.Diagnostics;
using GameEngine.DebugTools;
using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroGame.Screens;

namespace RetroGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Game
    {

#if WINDOWS || XBOX
        private static class Program
        {
            /// <summary>
            /// The main entry point for the application.
            /// </summary>
            private static void Main(string[] args)
            {
                using (var game = new Main())
                {
                    game.Run();
                }
            }
        }
#endif

        #region fields

        private readonly GraphicsDeviceManager  _graphics;
        private ScreenManager                   _screenManager;
        private bool                            _isDebug = false;
        int                                     _windowWidth;
        int                                     _windowHeight;

        #endregion

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // set the debug flag or not
            InitializeDebugMode();

            DebugSystem.Initialize(this, "SpriteFonts/gamefont");

            

#if WINDOWS_PHONE
			_graphics.IsFullScreen = true;
            _windowWidth = 800;
            _windowHeight = 480;
#else
            if (_isDebug)
            {
                _windowWidth = GraphicsDevice.DisplayMode.Width / 2;
                _windowHeight = GraphicsDevice.DisplayMode.Height / 2;
                _graphics.IsFullScreen = false;
            }
            else
            {
                _windowWidth = 1280;
                _windowHeight = 720;
                //_graphics.IsFullScreen = true;
            }
           
#endif


            // this ensures that we get a true 60fps call on update
            IsFixedTimeStep = true;
            // unfortunately this will cause the IsRunningSlowly flag to be true...
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f/65.0f); // 65 instead of 60 due to rounding
            _graphics.SynchronizeWithVerticalRetrace = true; // should be true

            // Setup frame buffer.
            _graphics.PreferredBackBufferWidth = _windowWidth;
            _graphics.PreferredBackBufferHeight = _windowHeight;
            _graphics.PreferMultiSampling = true;
            _graphics.ApplyChanges();

            // compute the scaleVector
            var scaleVector = new Vector2(_graphics.PreferredBackBufferWidth/1280f,
                                          _graphics.PreferredBackBufferHeight/720f);

            // Create a new instance of the Screen Manager. Have all drawing scaled from 720p to the PC's resolution
            _screenManager = new ScreenManager(this, scaleVector);


            // Activate the first screens.
            if(_isDebug)
                _screenManager.AddScreen(new PlanningGameplayScreen(), PlayerIndex.One);
            else
                _screenManager.AddScreen(new LogoScreen(), PlayerIndex.One);

            Components.Add(_screenManager);

            base.Initialize();
        }

        /// <summary>
        /// Set the debug mode to true
        /// </summary>
        [Conditional("DEBUG")]
        private void InitializeDebugMode()
        {
            _isDebug = true;
        }

        /// <summary>
        /// We need to override and set the background to black between screen transition
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            base.Draw(gameTime);
        }
    }
}