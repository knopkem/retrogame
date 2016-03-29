using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameEngine.DebugTools;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework.Content;
using TiledLib;

namespace RetroGame
{
    /// <summary>
    /// Parses maps for objects and converts properties to strong types
    /// Returns a collection of MapObjectExtended items
    /// </summary>
    internal class MapObjectConverter
    {
        public Collection<MapObjectExtended> MapObjects;

        private readonly MapHelper _mapHelper;

        /// <summary>
        /// Constructor needs an instance of mapHelper
        /// </summary>
        /// <param name="helper"></param>
        public MapObjectConverter(MapHelper helper)
        {
            _mapHelper = helper;
        }

        public void LoadContent(ContentManager content)
        {
            MapObjects = Convert(_mapHelper.GetObjects());
        }

        /// <summary>
        /// Creates an extended map object with default properties and status
        /// </summary>
        /// <param name="mapObj">object to inspect</param>
        /// <returns></returns>
        public MapObjectExtended CreateDefaultObject(MapObject mapObj)
        {
            var obj = new MapObjectExtended(mapObj);

            // assign our defaults
            var props = new PropertyList();

            // doors and windows
            if ((obj.ObjectType == EObjectTypes.Door) || (obj.ObjectType == EObjectTypes.Window))
            {
                props.Add(EObjectProperties.IS_BLOCKED, true);
                props.Add(EObjectProperties.IS_BROKEN, false);
                props.Add(EObjectProperties.IS_CLOSED, true);
                props.Add(EObjectProperties.IS_LOCKED, true);
                props.Add(EObjectProperties.IS_LOOTABLE, false);
            }            
                // safe
            else if (obj.ObjectType == EObjectTypes.Safe)
            {
                props.Add(EObjectProperties.IS_BLOCKED, true);
                props.Add(EObjectProperties.IS_BROKEN, false);
                props.Add(EObjectProperties.IS_CLOSED, true);
                props.Add(EObjectProperties.IS_LOCKED, true);
                props.Add(EObjectProperties.IS_SECURED, true);
                props.Add(EObjectProperties.IS_LOOTABLE, true);
            }
                // all others
            else
            {
                props.Add(EObjectProperties.IS_BLOCKED, true);
                props.Add(EObjectProperties.IS_BROKEN, false);
                props.Add(EObjectProperties.IS_CLOSED, true);
                props.Add(EObjectProperties.IS_LOCKED, true);
                props.Add(EObjectProperties.IS_LOOTABLE, false);
            }


            obj.SetNewProperties(props, 0f);

            return obj;
        }

        /// <summary>
        /// Parses an MapObject for properties and adds them to MapObjectExtended
        /// </summary>
        /// <param name="mapObjects">the object</param>
        /// <returns></returns>
        private Collection<MapObjectExtended> Convert(IEnumerable<MapObject> mapObjects)
        {
            var objects = new Collection<MapObjectExtended>();

            // parse and add properties to our objects
            foreach (MapObject mapObj in mapObjects)
            {
                MapObjectExtended obj = CreateDefaultObject(mapObj);

                char[] delimiterChars = {'#'};
                string status;
                try
                {
                    if (mapObj != null)
                    {
                        status = (string) mapObj.Properties["status"];
                        string[] statElements = status.Split(delimiterChars);
                    }

                }
                catch (Exception e)
                {
                    TraceLog.WriteWarning("Exception in MapObjectConverter.Convert: " + e.Message);
                }

                objects.Add(obj);
            }
            return objects;
        }
    }
}