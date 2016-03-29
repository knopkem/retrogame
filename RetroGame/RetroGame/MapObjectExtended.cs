using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.DebugTools;
using TiledLib;

namespace RetroGame
{
    /// <summary>
    /// Defines all properties objects can have
    /// </summary>
    public enum EObjectProperties
    {
        IS_CLOSED,
        IS_LOCKED,
        IS_SECURED,
        IS_BROKEN,
        IS_BLOCKED,
        IS_LOOTABLE
    }

    // no typedefs in c#, so we create a subclass
    public class PropertyList : Dictionary<EObjectProperties, bool>
    {
        public bool ReverseValue(EObjectProperties prop)
        {
            bool value;
            if (TryGetValue(prop, out value))
            {
                Remove(prop);
                Add(prop, !value);
                return true;
            }
            else
                return false;
        }

        public void Clone(PropertyList propsToClone)
        {
            Clear();
            foreach (var pair in propsToClone)
            {
                Add(pair.Key, pair.Value);
            }
        }
    }

    // we have to define IClonable ourself, since the phone api hides it
    internal interface ICloneable
    {
        // Methods
        object Clone();
    }


    /// <summary>
    /// Defines a map object including it's properties (things that could be done with it) and it's status (actual status)
    /// </summary>
    public class MapObjectExtended : MapObject, ICloneable
    {
        private readonly Dictionary<float, PropertyList> _timeToProps;

        /// <summary>
        /// Constructor takes object enum and converts it into a string (int)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objType"></param>
        public MapObjectExtended(string name, EObjectTypes objType) : base(name, objType.ToString())
        {
            _timeToProps = new Dictionary<float, PropertyList>();
        }

        /// <summary>
        /// CopyContructor
        /// </summary>
        /// <param name="copyFrom"></param>
        public MapObjectExtended(MapObject copyFrom)
            : base(copyFrom.Name, copyFrom.Type)
        {
            _timeToProps = new Dictionary<float, PropertyList>();
            Bounds = copyFrom.Bounds;
        }

        /// <summary>
        /// Type of object
        /// </summary>
        public EObjectTypes ObjectType
        {
#if WINDOWS
            get { return (EObjectTypes) Enum.Parse(typeof (EObjectTypes), Type); }
#else
            get {return (EObjectTypes) OpenNETCF.Enum2.Parse(typeof(EObjectTypes),Type); }
#endif
        }

        #region ICloneable Members

        /// <summary>
        /// Satisfy IClonable
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        /// <summary>
        /// Set new properties at a specific time
        /// </summary>
        /// <param name="props"></param>
        /// <param name="timeInMilliSeconds"></param>
        public void SetNewProperties(PropertyList props, float timeInMilliSeconds)
        {
            //TraceLog.Write("Setting properties at " + timeInMilliSeconds);
            if (!_timeToProps.ContainsKey(timeInMilliSeconds))
            {
                // we need to clone th dictionary to create different properties
                var newProps = new PropertyList();
                newProps.Clone(props);

                _timeToProps.Add(timeInMilliSeconds, newProps);
            }
        }

        /// <summary>
        /// Get the properties that match closely to the given time
        /// </summary>
        /// <param name="timeInMilliSeconds"></param>
        /// <returns></returns>
        public PropertyList GetProperties(float timeInMilliSeconds)
        {
            TraceLog.Write("Trying to find match at " + timeInMilliSeconds);
            TraceLog.Write("Object contains: " + _timeToProps.Count());
            var bestMatch = new KeyValuePair<float, PropertyList>(-1, new PropertyList());

            int counter = 0;
            foreach (var pair in _timeToProps)
            {
                if (pair.Key <= timeInMilliSeconds)
                    if (pair.Key > bestMatch.Key)
                    {
                        bestMatch = pair;
                        TraceLog.Write("Best match at:" + counter);
                    }
                counter++;
            }

            return bestMatch.Value;
        }
    }
}