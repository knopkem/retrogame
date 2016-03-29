using GameEngine.TileEngine;

namespace RetroGame
{
    /// <summary>
    /// This is the object that contains data that should be shared among screens
    /// </summary>
    internal struct SharedData
    {
        public CharPool GlobalCharPool;
        public MapHelper GlobalMapHelper;

        public MapObjectManager GlobalMapObjectManager;
    }
}