using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RetroGame
{
    /// <summary>
    /// Custom collector to store characters
    /// Should be improved to work with foreach
    /// </summary>
    internal class CharContainer : IEnumerable<Character2D>
    {
        private readonly Collection<Character2D> _charCollection;
        private int _currentIndex;

        public CharContainer()
        {
            _currentIndex = 0;
            _charCollection = new Collection<Character2D>();
        }

        public int Count
        {
            get { return _charCollection.Count; }
        }

        #region IEnumerable<Character2D> Members

        public IEnumerator<Character2D> GetEnumerator()
        {
            return _charCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public bool Contains(Character2D character)
        {
            return _charCollection.Contains(character);
        }

        public void Add(Character2D character)
        {
            _charCollection.Add(character);
        }

        public void Remove(Character2D character)
        {
            _charCollection.Remove(character);
        }

        /// <summary>
        /// iterate to next element
        /// </summary>
        public void Next()
        {
            if (_currentIndex == _charCollection.Count - 1)
                _currentIndex = 0;
            else
                _currentIndex++;
        }

        /// <summary>
        /// iterate to previous element
        /// </summary>
        public void Prev()
        {
            if (_currentIndex == 0)
                _currentIndex = _charCollection.Count - 1;
            else
                _currentIndex--;
        }

        /// <summary>
        /// Return the character at the current index
        /// </summary>
        /// <returns></returns>
        public Character2D GetCurrentChar()
        {
            return _charCollection.Count > _currentIndex ? _charCollection.ElementAt(_currentIndex) : null;
        }

        /// <summary>
        /// Returns the current index
        /// </summary>
        /// <returns></returns>
        public int GetCurrentIndex()
        {
            return _currentIndex;
        }

        /// <summary>
        /// Returns the character at the given index or the on close to it
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Character2D GetChar(int index)
        {
            MathHelper.Clamp(index, 0, _charCollection.Count - 1);
            return _charCollection.ElementAt(index);
        }
    }
}