using GameEngine.TileEngine;
using Microsoft.Xna.Framework.Content;
using RetroGameData;

namespace RetroGame
{
    /// <summary>
    /// Loads and holds all available characters and manages selected chars
    /// </summary>
    internal class CharPool
    {
        #region fields

        private const int MaxChars = 6;
        private readonly MapHelper _mapHelper;

        public CharContainer AvailableChars;
        public CharContainer SelectedChars;

        #endregion

        #region props

        public int CurrentCharacterIndex
        {
            get { return SelectedChars.GetCurrentIndex(); }
        }

        #endregion

        public CharPool(MapHelper mapHelper)
        {
            this._mapHelper = mapHelper;
            AvailableChars = new CharContainer();
            SelectedChars = new CharContainer();
        }


        /// <summary>
        /// select/deselect a char by using the character itself
        /// </summary>
        /// <param name="character"></param>
        public void SelectChar(Character2D character)
        {
            if (SelectedChars.Contains(character))
            {
                SelectedChars.Remove(character);
                character.IsSelected = false;
            }
            else
            {
                SelectedChars.Add(character);
                character.IsSelected = true;
            }
        }

        public void SelectCurrentChar()
        {
            SelectChar(AvailableChars.GetCurrentChar());
        }

        public Character2D GetCurrentCharacter()
        {
            return SelectedChars.GetCurrentChar();
        }

        /// <summary>
        /// return number of chars available in the pool
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfAvailableChars()
        {
            return AvailableChars.Count;
        }

        /// <summary>
        /// return number of chars the player selected
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfSelectedChars()
        {
            return SelectedChars.Count;
        }

        /// <summary>
        /// Load the chars from xap using the content pipeline
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            const string charName = "Characters/charProp_";
            for (int i = 0; i < MaxChars; i++)
            {
                // loading properties of all characters from xml in content
                var props = content.Load<CharProperties>(charName + (i + 1));
                var player = new Character2D(_mapHelper, props);
                player.LoadContent(content);
                AvailableChars.Add(player);
            }
        }

        /// <summary>
        /// Serial all available characters (props only) to xml
        /// </summary>
        public void SerializeAllChars()
        {
            foreach (Character2D character in AvailableChars)
                character.SerializeProperties();
        }
    }
}