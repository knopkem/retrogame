using System.Collections.ObjectModel;
using System.Linq;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace RetroGame
{
    /// <summary>
    /// Holds all extended MapObjects found in the map and manages its status while the game is progressing
    /// You can retrieve an object based on time
    /// </summary>
    internal class MapObjectManager
    {
        private readonly MapObjectConverter   _converter;
        private MapHelper                     _mapHelper;
        private Collection<MapObjectExtended> _objects;
        private MapObjectExtended             _storedObject;
        private Vector2                       _storedPos;

        public MapObjectManager(MapHelper mapHelper)
        {
            this._mapHelper = mapHelper;
            _converter = new MapObjectConverter(mapHelper);
        }

        public void LoadContent(ContentManager content)
        {
            // get the initial objects with default values applied
            _converter.LoadContent(content);
            _objects = _converter.MapObjects;
        }

        /// <summary>
        /// Try to find the object at this position and time and return it
        /// </summary>
        /// <param name="pos">position in map</param>   
        /// <returns>a map object</returns>
        public MapObjectExtended GetObjectAtPosition(Vector2 pos)
        {
            // check if we can return the last object
            if (_storedPos == pos)
                return _storedObject;
/*
            // try to find it in our map storage
            foreach (MapObjectExtended obj in _objects)
            {
                if (obj.Bounds.Contains(new Point((int) pos.X, (int) pos.Y)))
                {
                    returnObj = obj;
                    break;
                }
            }
 */

            // try to find it in our map storage
            MapObjectExtended returnObj = _objects.FirstOrDefault(obj => obj.Bounds.Contains(new Point((int) pos.X, (int) pos.Y)));

            // return the default object 
            if (returnObj == null)
                returnObj = new MapObjectExtended("NULL", EObjectTypes.NoObject);

            // safe last pair
            _storedObject = returnObj;
            _storedPos = pos;

            return returnObj;
        }
    }
}