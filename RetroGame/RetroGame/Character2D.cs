using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using GameEngine.DebugTools;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RetroGameData;
// xna
// custom

namespace RetroGame
{
    /// <summary>
    /// A character represents a plan able team member that holds properties
    /// The class should be serialization
    /// </summary>
    internal class Character2D : Character
    {
        #region public fields

        public bool IsSelected;

        /// <summary>
        /// The recored list of tasks including way points
        /// </summary>
        public List<Task> TaskList;

        /// <summary>
        /// The dynamic properties that each player has that modify the time it takes to do tasks
        /// </summary>
        public CharProperties CharProps;

        /// <summary>
        /// The cursor for the end of the path
        /// </summary>
        public Cursor Cursor;

        public Rectangle PhotoRectangle;

        protected Texture2D SpritePoint;
        protected Texture2D SpriteWaypoint;
        protected Vector2 DrawOriginWaypoint;

        #endregion

        #region private fields

        private const float SpeedInInteractionMode = 2;
        private const float SpeedInPlanningMode = 400;

        /// <summary>
        /// Indicates the position in the task list
        /// </summary>
        private int _currentTaskIndex;

        /// <summary>
        /// indicates if we are running our loop
        /// </summary>
        private bool _isExecutingTasks;

        /// <summary>
        /// The pathCalculator generates lists of tiles that need to be traveled
        /// </summary>
        private PathCalculator _pathCalc;

        private Point _playerSize;
        private Rectangle _sourceRect;
        private Vector2 _spriteDrawOrigin;
        private Vector2 _drawOriginFrame;
        private Texture2D _spriteFrame;
        private double _currentTime;
        private SpriteFont _cursorFont;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="map"></param>
        /// <param name="props"></param>
        public Character2D(MapHelper map, CharProperties props)
            : base(map, props)
        {
            // initialize defaults
            CharProps = props;

            TaskList = new List<Task>();

            Speed = SpeedInPlanningMode;
        }

        /// <summary>
        /// Helper to Print the properties of this char
        /// </summary>
        public void PrintProperties()
        {
            TraceLog.Write("Name: " + CharProps.CharName);
            TraceLog.Write("LockPicking: " + CharProps.LockPicking);
            TraceLog.Write("Jamming: " + CharProps.Jamming);
            TraceLog.Write("ForceLock: " + CharProps.ForceLock);
            TraceLog.Write("Stealth: " + CharProps.Stealth);
            TraceLog.Write("Strength: " + CharProps.Strength);
        }

        /// <summary>
        /// Returns the time it takes to execute the task
        /// </summary>
        /// <param name="etask">Taskobject</param>
        /// <returns>time</returns>
        public float GetTimeForTask(ETasks etask)
        {
            Task task = CommonHelper.ETaskToTask(etask);
            return CommonHelper.GetCalculatedTimeForTask(task, CharProps);
        }

        /// <summary>
        /// Add a new task to this char
        /// </summary>
        /// <param name="etask"></param>
        public void AddTask(ETasks etask)
        {
            Task task = CommonHelper.ETaskToTask(etask);
            TraceLog.Write("Player " + CharProps.CharName + " got new task " + task.Name);
            TraceLog.Write("Task takes: " + CommonHelper.GetCalculatedTimeForTask(task, CharProps) + " seconds");

            // auto compute the path between the object and the player
            AddRunTask(etask);

            // all non moving tasks create tasks with empty paths
            if (etask != ETasks.Running && etask != ETasks.Sneaking)
            {
                // add it to the task list
                var storeTask = new Task(task);
                // attach the path to the task
                storeTask.Path = new List<Waypoint>();
                storeTask.TimeEstimated = CommonHelper.GetCalculatedTimeForTask(storeTask, CharProps);

                TaskList.Add(storeTask);
            }
        }

        /// <summary>
        /// Set the current path between the player and the cursor
        /// </summary>
        private void AddRunTask(ETasks etask)
        {
            if (Cursor.Position == Position)
                return;

            SetWalkpath(_pathCalc.GetComputedPath());

            Task task;

            if ((etask == ETasks.Running) || (etask == ETasks.Sneaking))
            {
                // we reuse the one that was set
                task = CommonHelper.ETaskToTask(etask);
            }
            else
            {
                // Create a new task, depending on the mode we are in
                task = CommonHelper.ETaskToTask(ETasks.Running);
            }
            var storeTask = new Task(task);

            // attach the path to the task
            storeTask.Path = _pathCalc.GetComputedPath();
            storeTask.TimeEstimated = CommonHelper.GetCalculatedTimeForTask(storeTask, CharProps);

            // add it to the task list
            TaskList.Add(storeTask);
        }


        /// <summary>
        /// Returns the time it takes to execute all stored jobs
        /// </summary>
        /// <returns></returns>
        public float GetTotalTimeOfTaskList()
        {
            // sum all estimated times and return it (LINQ)
            return TaskList.Sum(task => task.TimeEstimated);
        }

        /// <summary>
        /// Return the time it takes to perform until index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float GetTimeUntilTaskIndex(int index)
        {
            float totalTime = 0;
            if (!(index < TaskList.Count))
                index = TaskList.Count - 1;

            for (int i = 0; i <= index; i++)
            {
                totalTime += TaskList[i].TimeEstimated;
            }
            return totalTime;
        }

        /// <summary>
        /// Returns the time it will take to travel along the current path
        /// </summary>
        /// <returns></returns>
        public float GetTimeOfActivePath()
        {
            return CommonHelper.TimeToTravelPath(_pathCalc.GetComputedPath(), SpeedInInteractionMode);
        }

        /// <summary>
        /// Start from the beginning and execute all tasks (deactivates the default movement)
        /// </summary>
        public void ExecuteTaskList()
        {
            Speed = SpeedInInteractionMode;
            _currentTaskIndex = 0;
            _isExecutingTasks = true;
            _currentTime = 0;
            SetNextTask();
        }

        /// <summary>
        /// Reset to planning mode
        /// </summary>
        public void StopExecutingTaskList()
        {
            _isExecutingTasks = false;
            Speed = SpeedInPlanningMode;
        }

        /// <summary>
        /// Retrieves the stored path from Task at current TaskIndex
        /// Makes sure that the path is empty and valid in case it is missing
        /// </summary>
        /// <returns></returns>
        private List<Waypoint> GetPathFromTaskList()
        {
            if ((_currentTaskIndex < TaskList.Count) && (_currentTaskIndex >= 0))
                return TaskList[_currentTaskIndex].Path;
            else
                return new List<Waypoint>();
        }

        /// <summary>
        /// Returns the current task, boundary check included
        /// </summary>
        /// <returns></returns>
        private Task GetCurrentTaskFromTaskList()
        {
            if (_currentTaskIndex < TaskList.Count && _currentTaskIndex >= 0)
                return TaskList[_currentTaskIndex];
            else
                return new Task(ETasks.Waiting, "Wait", 1);
        }

        /// <summary>
        /// Set the next task as current, stops the active one immediately
        /// </summary>
        private void SetNextTask()
        {
            TraceLog.Write("Task " + _currentTaskIndex);
            List<Waypoint> path = GetPathFromTaskList();
            if (path.Count > 0 && _currentTaskIndex == 0)
                Position = path[0].Position;
            
            // get the speed of the current task
            if (GetCurrentTaskFromTaskList().TaskType == ETasks.Running ||
                GetCurrentTaskFromTaskList().TaskType == ETasks.Sneaking)
                Speed = GetCurrentTaskFromTaskList().DefaultTimeInSeconds;
            SetWalkpath(path);
            _currentTaskIndex++;
        }

        /// <summary>
        /// Write properties of this player to xml (experimental)
        /// </summary>
        public void SerializeProperties()
        {
#if WINDOWS
            string path = Directory.GetCurrentDirectory();
            var writer = new StreamWriter(path + "\\charProp_" + CharProps.TextureAssetNumber + "Props.xml");
            var xs = new XmlSerializer(typeof (CharProperties));
            xs.Serialize(writer, CharProps);
            writer.Close();
#endif
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            SpritePoint = content.Load<Texture2D>("OtherTextures/whiteblock");
            SpriteWaypoint = content.Load<Texture2D>("OtherTextures/cursor_small");
            _spriteFrame = content.Load<Texture2D>("OtherTextures/cursor");
            _cursorFont = content.Load<SpriteFont>("SpriteFonts/gamefont");

            DrawOriginWaypoint = new Vector2((float)MapHelperObj.GameMap.TileWidth/2, (float)MapHelperObj.GameMap.TileHeight/2);

            // init default stuff
            _playerSize = new Point(MapHelperObj.GameMap.TileWidth*3, MapHelperObj.GameMap.TileHeight*3);
            _sourceRect = new Rectangle(0, 0, SpriteTexture2D.Width, SpriteTexture2D.Height - 100);
            _spriteDrawOrigin = new Vector2((float)SpriteTexture2D.Width/2, ((float)SpriteTexture2D.Height - 100)/2);
            _drawOriginFrame = new Vector2((float)_spriteFrame.Width/2, (float)_spriteFrame.Height/2);

            // init the cursor
            Cursor = new Cursor(MapHelperObj, Position, "OtherTextures/cursor");
            Cursor.LoadContent(content);
            Cursor.CheckForCollision = false;
            Cursor.Speed = 4;

            // init the path calculator
            _pathCalc = new PathCalculator(MapHelperObj, Vector2.Zero, "OtherTextures/whiteblock");
            _pathCalc.LoadContent(content);
        }

        public override void Update(GameTime gameTime, Camera2D gameCamera)
        {
            // planning mode only
            if (!_isExecutingTasks)
            {
                // the controllable object uses way points and speed to navigate the player
                base.Update(gameTime, gameCamera);

                // update cursor and path
                Cursor.Update(gameTime, gameCamera);
                _pathCalc.Update(Position, Cursor.Position);
            }
            else
            {
                // lock the cursor to the player
                Cursor.Position = Position;

                // we are playing the recorded tasks
                if (_currentTaskIndex < TaskList.Count)
                {
                    double timeForTask = GetTimeUntilTaskIndex(_currentTaskIndex - 1);
                    _currentTime += gameTime.ElapsedGameTime.TotalSeconds;

                    // if the estimated time for this path is over we need to switch to the next
                    if (timeForTask < _currentTime)
                    {
                        TraceLog.Write("timeForLastTask" + timeForTask + " < " + _currentTime);
                        if (CurrentIndex < CurrentWalkpath.Count - 1)
                            TraceLog.Write("skipping path");

                        SetNextTask();
                    }
                }

                // the controllable object uses way points and speed to navigate the player
                base.Update(gameTime, gameCamera);
            }
        }

        /// <summary>
        /// Takes the picture from the char and positions using the pos and size of the player
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="spriteBatch"></param>
        public void DrawPhoto(Vector2 pos, SpriteBatch spriteBatch)
        {
            // draw the player
            PhotoRectangle = new Rectangle((int) pos.X, (int) pos.Y, _playerSize.X, _playerSize.Y);
            spriteBatch.Draw(SpriteTexture2D, PhotoRectangle, _sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw the character
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw the player
            var destRect = new Rectangle((int) Position.X, (int) Position.Y, _playerSize.X, _playerSize.Y);
            spriteBatch.Draw(SpriteTexture2D, destRect, _sourceRect, Color.White, 0f, _spriteDrawOrigin, SpriteEffects.None, 0);
            
            // draw a frame around it
            spriteBatch.Draw(_spriteFrame, Position, null, Color.White, 0f, _drawOriginFrame, DrawScale,
                             SpriteEffects.None, 0);

            // only show the cursor and path in planning mode
            if (!_isExecutingTasks)
            {
                // Draw the cursor
                Cursor.Draw(spriteBatch);

                // Draw Path between cursor and player
                _pathCalc.Draw(spriteBatch);

                // draw recorded path
                foreach (Waypoint wayPoint in RecordedPath)
                {
                    if (wayPoint.IsStoredWaypoint)
                        spriteBatch.Draw(SpriteWaypoint, wayPoint.Position, null, Color.Blue, 0f,
                                         DrawOriginWaypoint, 1, SpriteEffects.None, 0);

                    else
                        spriteBatch.Draw(SpritePoint, wayPoint.Position, Color.Blue);
                }

                // draw time of step on cursor
                spriteBatch.DrawString(_cursorFont, this.GetTimeOfActivePath().ToString("G2") + "s",
                                       this.Cursor.Position + new Vector2(30, -30), Color.DarkRed);

                spriteBatch.DrawString(_cursorFont,
                                       (this.GetTotalTimeOfTaskList() + this.GetTimeOfActivePath()).ToString("G2") + "s",
                                       this.Cursor.Position + new Vector2(30, -10), Color.DarkBlue);
            }
        }
    }
}