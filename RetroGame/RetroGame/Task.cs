using System.Collections.Generic;
using GameEngine.TileEngine;

namespace RetroGame
{
    /// <summary>
    /// A task is mission that can be assigned to task list of a player
    /// </summary>
    public class Task
    {
        public float DefaultTimeInSeconds;
        public string Name;
        public ETasks TaskType;
        public float TimeEstimated;

        private readonly List<Waypoint> _path;
        
        public Task(ETasks taskType, string name, float time)
        {
            TaskType = taskType;
            Name = name;
            DefaultTimeInSeconds = time;
            TimeEstimated = 0; //undefined
            Path = new List<Waypoint>();
        }

        public Task(Task copyTask)
        {
            TaskType = copyTask.TaskType;
            Name = copyTask.Name;
            DefaultTimeInSeconds = copyTask.DefaultTimeInSeconds;
            TimeEstimated = copyTask.TimeEstimated;
            _path = new List<Waypoint>();
        }

        public List<Waypoint> Path
        {
            get { return _path; }

            set
            {
                foreach (Waypoint wp in value)
                    _path.Add(wp);
            }
        }
    }
}