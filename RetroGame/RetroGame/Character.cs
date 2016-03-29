using GameEngine.TileEngine;
using RetroGameData;

namespace RetroGame
{
    /// <summary>
    /// Common functionality for all character classes
    /// </summary>
    internal class Character : ControllableObject
    {
        public Character(MapHelper map, CharProperties props)
            : base(map, props.StartPosition, "Characters/char_" + props.TextureAssetNumber)
        {
        }
    }
}