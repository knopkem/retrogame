using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RetroGameData
{
    /// <summary>
    /// This is the structure of the character data that is 
    /// serialized to provide different stats to each character
    /// </summary>
    public class CharProperties
    {
        // general
        public string CharName;
        public Vector2 StartPosition;
        public int TextureAssetNumber;

        // job skills
        public float LockPicking;
        public float SafeCracking;
        public float Jamming;
        public float ForceLock;

        // character skills
        public float Stealth;
        public float Strength;

        public List<string> FullTextStringList;
    }
}
