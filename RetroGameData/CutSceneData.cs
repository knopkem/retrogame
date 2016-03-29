using System.Collections.Generic;

namespace RetroGameData
{
    /// <summary>
    /// The is the structure of the cut scenes
    /// </summary>
    public class CutSceneData
    {
        /// <summary>
        /// contains all single text string lists
        /// </summary>
        public List<string> StoryText;

        /// <summary>
        /// contains all image asset names
        /// </summary>
        public List<string> ImageAssetName;

        /// <summary>
        /// contains all image description, need to be exactly the same as the image asset names
        /// </summary>
        public List<string> ImageDescription;
    }
}
