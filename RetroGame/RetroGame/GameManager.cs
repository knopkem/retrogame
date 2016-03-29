using System;
using GameEngine.DebugTools;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework.Content;

namespace RetroGame
{
    /// <summary>
    /// Singleton class that handles game data between screens and current game status
    /// </summary>
    internal sealed class GameManager
    {
        private static readonly GameManager _instance = new GameManager();
        public bool IsInitialized;

        private ContentManager _content;
        private SharedData _sharedData;

        private GameManager()
        {
            _sharedData = new SharedData();
        }

        public static GameManager Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// return the members important in game screens
        /// </summary>
        public SharedData Data
        {
            get { return _sharedData; }
        }

        /// <summary>
        /// All initializing goes here
        /// </summary>
        /// <param name="content"></param>
        public void Initialize(ContentManager content)
        {
            _content = content;
            IsInitialized = true;
        }

        /// <summary>
        /// Create a new game loading the first map and initialize everything
        /// </summary>
        public void StartNewGame()
        {
            if (!IsInitialized)
            {
                TraceLog.Write("GameManager not initialized");
                return;
            }

            TraceLog.Write("GameManager starting new game.");

            // load first level
            // load the map helper with the map
            _sharedData.GlobalMapHelper = new MapHelper("LevelMaps/Level1");
            _sharedData.GlobalMapHelper.LoadContent(_content);

            // load char pool
            _sharedData.GlobalCharPool = new CharPool(_sharedData.GlobalMapHelper);
            _sharedData.GlobalCharPool.LoadContent(_content);

            // load objectManager
            _sharedData.GlobalMapObjectManager = new MapObjectManager(_sharedData.GlobalMapHelper);
            _sharedData.GlobalMapObjectManager.LoadContent(_content);
        }

        /// <summary>
        /// Deserialize the game
        /// </summary>
        public void ResumeGame()
        {
            if (!IsInitialized)
                return;
            TraceLog.Write("GameManager resume game not implemented");
        }
    }
}