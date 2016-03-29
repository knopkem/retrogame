using System;
using System.Collections.Generic;
using GameEngine.DebugTools;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using RetroGameData;

namespace RetroGame
{
    //-----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines all types of objects that can be present in a map
    /// </summary>
    public enum EObjectTypes
    {
        NoObject = 0,
        Door = 1,
        Window = 2,
        Desk = 3,
        Till = 4,
        Alarm = 5,
        Vitrine = 6,
        Sink = 7,
        Safe = 8
    }

    //-----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Defines all task that can be assigned to a player
    /// </summary>
    public enum ETasks
    {
        Running,
        Sneaking, // running but at half speed
        Waiting,

        Open,
        Close,

        LockPicking,
        ForceLock,
        LockUp,

        Break,
        Repair,

        Disable,
        Enable,

        Take
    }

    //-----------------------------------------------------------------------------------------------------------------

    internal static class CommonHelper
    {
        public static List<ETasks> Tasks = new List<ETasks>();

        public static List<string> ResultStringList = new List<string>();

        private static readonly Dictionary<ETasks, Task> TaskDictionary = new Dictionary<ETasks, Task>
        {
            {
                ETasks.Running,
                new Task(ETasks.Running, "Run To",
                        2)
                },
            // time to get from one tile to the next
            {
                ETasks.Sneaking,
                new Task(ETasks.Sneaking,
                        "Sneak To", 1)
                },
            {
                ETasks.Waiting,
                new Task(ETasks.Waiting, "Wait",
                        10)
                },
            {
                ETasks.Open,
                new Task(ETasks.Open, "Open", 2)
                },
            {
                ETasks.Close,
                new Task(ETasks.Close, "Close", 2)
                },
            {
                ETasks.LockPicking,
                new Task(ETasks.LockPicking,
                        "Pick Lock", 15)
                },
            {
                ETasks.ForceLock,
                new Task(ETasks.ForceLock,
                        "Force Lock", 10)
                },
            {
                ETasks.LockUp,
                new Task(ETasks.LockUp, "Lock Up",
                        5)
                },
            {
                ETasks.Break,
                new Task(ETasks.Break, "Break", 8)
                },
            {
                ETasks.Repair,
                new Task(ETasks.Repair, "Repair",
                        30)
                },
            {
                ETasks.Disable,
                new Task(ETasks.Disable,
                        "Disable Alarm", 15)
                },
            {
                ETasks.Enable,
                new Task(ETasks.Enable,
                        "Enable Alarm", 15)
                },
            {
                ETasks.Take,
                new Task(ETasks.Take, "Take Loot",
                        5)
                }
        };

        //-----------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<EObjectTypes, MapObjectExtended> ObjectDictionary = new Dictionary
            <EObjectTypes, MapObjectExtended>
        {
            {
                EObjectTypes.
                NoObject,
                new MapObjectExtended
                ("Empty Space",
                EObjectTypes
                    .
                    NoObject)
                },
            {
                EObjectTypes.
                Door,
                new MapObjectExtended
                ("Door",
                EObjectTypes
                    .Door)
                },
            {
                EObjectTypes.
                Window,
                new MapObjectExtended
                ("Window",
                EObjectTypes
                    .Window)
                },
            {
                EObjectTypes.
                Desk,
                new MapObjectExtended
                ("Desk",
                EObjectTypes
                    .Desk)
                },
            {
                EObjectTypes.
                Till,
                new MapObjectExtended
                ("Till",
                EObjectTypes
                    .Till)
                },
            {
                EObjectTypes.
                Alarm,
                new MapObjectExtended
                ("Alarm",
                EObjectTypes
                    .Alarm)
                },
            {
                EObjectTypes.
                Vitrine,
                new MapObjectExtended
                ("Vitrine",
                EObjectTypes
                    .Vitrine)
                },
            {
                EObjectTypes.
                Sink,
                new MapObjectExtended
                ("Sink",
                EObjectTypes
                    .Sink)
                },
            {
                EObjectTypes.
                Safe,
                new MapObjectExtended
                ("Safe",
                EObjectTypes
                    .Safe)
                }
        };


        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Mostly needed to build up dynamic menus
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static List<string> ConvertTasksToNames(List<ETasks> tasks)
        {
            ResultStringList.Clear();
            foreach (ETasks task in tasks)
                ResultStringList.Add(ETaskToName(task));
            return ResultStringList;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert an object enum to real type
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static MapObjectExtended EObjectToObject(EObjectTypes objType)
        {
            MapObjectExtended value;

            return ObjectDictionary.TryGetValue(objType, out value) ? value : null;
        }

        //-----------------------------------------------------------------------------------------------------------------

        public static string EObjectToName(EObjectTypes objType)
        {
            MapObjectExtended obj = EObjectToObject(objType);

            return obj != null ? obj.Name : "Empty Space";
        }

        //-----------------------------------------------------------------------------------------------------------------

        public static List<ETasks> GetAvailableTasks(MapObjectExtended obj, float time)
        {
            Tasks.Clear();

            PropertyList props = obj.GetProperties(time);
            bool canContinue = true;
            bool isOpen = false;


            if (props != null)
            {
                // if object can be broken
                bool value;
                if (props.TryGetValue(EObjectProperties.IS_BROKEN, out value) && canContinue)
                {
                    if (value)
                    {
                        Tasks.Add(ETasks.Repair);
                        canContinue = false;
                        isOpen = true;
                    }
                    else
                    {
                        // only if closed
                        props.TryGetValue(EObjectProperties.IS_CLOSED, out value);
                        if (value)
                        {
                            Tasks.Insert(0, ETasks.Break);
                        }
                    }
                }

                // secured
                if (props.TryGetValue(EObjectProperties.IS_SECURED, out value) && canContinue)
                {
                    if (value)
                    {
                        Tasks.Insert(0, ETasks.Disable);
                    }
                    else
                    {
                        // only if closed
                        props.TryGetValue(EObjectProperties.IS_CLOSED, out value);
                        if (value)
                            Tasks.Insert(0, ETasks.Enable);
                    }
                }

                // locked
                if (props.TryGetValue(EObjectProperties.IS_LOCKED, out value) && canContinue)
                {
                    if (value)
                    {
                        Tasks.Insert(0, ETasks.ForceLock);
                        Tasks.Insert(1, ETasks.LockPicking);
                        canContinue = false;
                    }
                    else
                    {
                        // only if closed
                        props.TryGetValue(EObjectProperties.IS_CLOSED, out value);
                        if (value)
                        {
                            Tasks.Insert(0, ETasks.LockUp);
                        }
                    }
                }

                // closed
                if (props.TryGetValue(EObjectProperties.IS_CLOSED, out value) && canContinue)
                {
                    if (value)
                    {
                        Tasks.Insert(0, ETasks.Open);
                    }
                    else
                    {
                        isOpen = true;
                        Tasks.Insert(0, ETasks.Close);
                    }
                }


                // take loot
                if (props.TryGetValue(EObjectProperties.IS_LOOTABLE, out value) && isOpen)
                {
                    if (value)
                        Tasks.Insert(0, ETasks.Take);
                }

                // defaults
                Tasks.Add(ETasks.Running);
                Tasks.Add(ETasks.Sneaking);
                Tasks.Add(ETasks.Waiting);
            }


            return Tasks;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Set new object properties after processing the object with this task
        /// </summary>
        /// <param name="etask"></param>
        /// <param name="time"></param>
        /// <param name="obj"></param>
        public static void ProcessObject(ETasks etask, float time, ref MapObjectExtended obj)
        {
            PropertyList list = obj.GetProperties(time);
            var newList = new PropertyList();
            newList.Clone(list);

            // Close/Open
            if ((etask == ETasks.Close) || (etask == ETasks.Open))
            {
                newList.ReverseValue(EObjectProperties.IS_CLOSED);
            }

            // lock/unlock
            if ((etask == ETasks.ForceLock) || (etask == ETasks.LockPicking) || (etask == ETasks.LockUp))
            {
                newList.ReverseValue(EObjectProperties.IS_LOCKED);
            }

            // secure/disable
            if ((etask == ETasks.Enable) || (etask == ETasks.Disable))
            {
                newList.ReverseValue(EObjectProperties.IS_SECURED);
            }

            // break/repair
            if ((etask == ETasks.Break) || (etask == ETasks.Repair))
            {
                newList.ReverseValue(EObjectProperties.IS_BROKEN);
            }

            // loot
            if (etask == ETasks.Take)
            {
                newList.ReverseValue(EObjectProperties.IS_LOOTABLE);
            }

            // finally set the new object properties
            obj.SetNewProperties(newList, time);
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the tasks at a given time for this map-object that can be assigned to a player
        /// </summary>
        /// <param name="obj">the object</param>
        /// <param name="time">time in milliseconds</param>
        /// <returns>string with task names</returns>
        public static List<string> GetAvailableTasksAsStringList(MapObjectExtended obj, float time)
        {
            return ConvertTasksToNames(GetAvailableTasks(obj, time));
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert a task enum to a task name
        /// </summary>
        /// <param name="etask">the enum</param>
        /// <returns>the task</returns>
        public static string ETaskToName(ETasks etask)
        {
            Task value = ETaskToTask(etask);

            if (value == null)
                return "KeyNotFoundError";
            else
                return value.Name;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert a task.Name to Etask enum
        /// </summary>
        /// <param name="name">the name of the task</param>
        /// <returns>the enum</returns>
        public static ETasks NameToTaskE(string name)
        {
            foreach (var pair in TaskDictionary)
            {
                if (name.Equals(pair.Value.Name))
                    return pair.Key;
            }

            return ETasks.Waiting;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert a task enum to a task
        /// </summary>
        /// <param name="etask"></param>
        /// <returns></returns>
        public static Task ETaskToTask(ETasks etask)
        {
            Task value;

            if (TaskDictionary.TryGetValue(etask, out value))
                return value;

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This should take the task type and properties of the char into account and calculate the time it takes to perform the task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static float GetCalculatedTimeForTask(Task task, CharProperties props)
        {
            float time;

            switch (task.TaskType)
            {
                case ETasks.Running:
                    time = TimeToTravelPath(task.Path, task.DefaultTimeInSeconds);
                    break;
                case ETasks.Sneaking:
                    time = TimeToTravelPath(task.Path, task.DefaultTimeInSeconds);
                    break;
                case ETasks.LockPicking:
                    time = EstimateTime(task.DefaultTimeInSeconds, props.LockPicking);
                    break;
                case ETasks.Break:
                    time = EstimateTime(task.DefaultTimeInSeconds, props.Jamming);
                    break;
                case ETasks.ForceLock:
                    time = EstimateTime(task.DefaultTimeInSeconds, props.ForceLock);
                    break;

                default:
                    TraceLog.Write("Task not calculated, default time taken!");
                    time = task.DefaultTimeInSeconds;
                    break;
            }

            return time;
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This is the most crucial method that defines the impact of modifiers to the time it takes to perform the task
        /// </summary>
        /// <param name="time"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        private static float EstimateTime(float time, float modifier)
        {
            return time - (time/100*(modifier*10/2));
        }

        //-----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Return the time it will take for the player to travel the path, given the current speed
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="speed">speed to travel</param>
        /// <returns>time in milliseconds</returns>
        public static float TimeToTravelPath(List<Waypoint> path, float speed)
        {
            float time = 0;
            float length = 0;

            if (path == null)
                return 0;

            for (int i = 0; i < path.Count; i++)
            {
                if (i + 1 < path.Count)
                {
                    Vector2 vect1 = path[i].Position;
                    Vector2 vect2 = path[i + 1].Position;
                    length += Vector2.Distance(vect2, vect1);
                }
            }

            time = length/(speed*60); // target frame rate is 60 fps

            return time;
        }

        //-----------------------------------------------------------------------------------------------------------------
    }
}