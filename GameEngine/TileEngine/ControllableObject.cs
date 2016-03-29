using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.DebugTools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// Object that can be controlled in 2D and 3D space
    /// </summary>
    public class ControllableObject
    {
        public bool                 CheckForCollision;
        public Vector2              DrawOrigin;
        public Vector2              Position;
        public Vector2              Target;
        public string               TextureAssetName;
        public MapHelper            MapHelperObj;
        public Texture2D            SpriteTexture2D;
        public float                DrawScale = 1f;
        public float                Speed = 4;
        
        protected int               CurrentIndex;
        protected List<Waypoint>    CurrentWalkpath;
        protected List<Waypoint>    RecordedPath;
       
        private float               _elapsedTime;
        private Vector2             _faceDirection;
       
        #region Public

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="assetName"></param>
        public ControllableObject(MapHelper map, Vector2 pos, string assetName)
        {
            Position = pos;
            Target = Position;
            MapHelperObj = map;
            TextureAssetName = assetName;
            CurrentWalkpath = new List<Waypoint>();
            RecordedPath = new List<Waypoint>();
        }

        /// <summary>
        /// Load the texture and get the origin
        /// </summary>
        /// <param name="content"></param>
        public virtual void LoadContent(ContentManager content)
        {
            SpriteTexture2D = content.Load<Texture2D>(TextureAssetName);
            DrawOrigin = new Vector2((float)SpriteTexture2D.Width/2, (float)SpriteTexture2D.Height/2);
        }

        /// <summary>
        /// Unload
        /// </summary>
        public void UnloadContent()
        {
            SpriteTexture2D = null;
        }

        /// <summary>
        /// Set a new path between the player and the cursor.
        /// </summary>
        /// <param name="path"></param>
        public virtual void SetWalkpath(List<Waypoint> path)
        {
            CurrentWalkpath.Clear();
            for (int i = 0; i < path.Count; i++)
            {
                CurrentWalkpath.Add(path.ElementAt(i));
                RecordedPath.Add(path.ElementAt(i));

                // the last waypoint is a real waypoint
                if (i == 0)
                    CurrentWalkpath.ElementAt(i).IsStoredWaypoint = true;
            }
            CurrentIndex = 0;
            _elapsedTime = 0;
            TraceLog.Write("Way points: " + CurrentWalkpath.Count);
        }

        /// <summary>
        /// Return the direction the player should look
        /// </summary>
        protected Vector2 GetFaceDirection()
        {
            return _faceDirection;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="gameCamera"></param>
        public virtual void Update(GameTime gameTime, Camera2D gameCamera)
        {
            // automatic position calculation based on waypoints
            if ((CurrentWalkpath != null) && (CurrentWalkpath.Count > 0) && (CurrentIndex < CurrentWalkpath.Count))
            {
                _elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
                Vector2 nextPosition = GetWaypointPositionAtTime(_elapsedTime/1000);
                _faceDirection = new Vector2(Position.X - nextPosition.X, Position.Y - nextPosition.Y);
                Position = nextPosition;
                Target = Position;
                //TraceLog.Write("currentIndex: " + currentIndex + " # " + (currentWalkpath.Count -1));
            }
            else if (Position != Target)
            {
                // Move toward the target vector
                Vector2 moveVect = Target - Position;
                moveVect.Normalize();
                Vector2 addPosition = moveVect*Speed;

                // we need to know in advance if the targertTile is blocked
                bool isBlocked = false;
                if (CheckForCollision)
                    isBlocked = MapHelperObj.IsAreaBlocked(Position + addPosition);

                if (!isBlocked)
                {
                    // Move along the movement vector at our given speed
                    Position += addPosition;

                    // Floats will never be exactly equal, so set position to target when we're close enough.
                    if (Vector2.Distance(Position, Target) < Speed)
                    {
                        // we only need to correct the position if we are at our endpoint (this is a bug!)
                        Position = Target;
                    }
                }
            }


            // Clamp to camera bounds
            Position.X = MathHelper.Clamp(Position.X, gameCamera.ClampRect.Left, gameCamera.ClampRect.Right);
            Position.Y = MathHelper.Clamp(Position.Y, gameCamera.ClampRect.Top, gameCamera.ClampRect.Bottom);
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteTexture2D, Position, null, Color.White, 0f, DrawOrigin, DrawScale, SpriteEffects.None, 0);
        }

        #endregion

        #region Private

        /// <summary>
        /// Return interpolated position between two way points
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Vector2 GetPlayersPosition(Vector2 pos1, Vector2 pos2, float t)
        {
            if (t > 1.0f)
                t = 1.0f;
            if (t < 0.0f)
                t = 0.0f;

            return Vector2.Lerp(pos1, pos2, t);
        }

        /// <summary>
        /// Find the index that fits best to the elapsed time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private Vector2 GetWaypointPositionAtTime(float time)
        {
            float timeSpend = 0;
            int index = 0;
            Vector2 startPos = CurrentWalkpath.ElementAt(0).Position;
            Vector2 lastWpPos = Vector2.Zero;
            lastWpPos.X = startPos.X;
            lastWpPos.Y = startPos.Y;
            float summedDistance = 0;
            float t = 0;
            //foreach (Waypoint wp in currentWalkpath)
            for (int i = 0; i < CurrentWalkpath.Count; i++)
            {
                Waypoint wp = CurrentWalkpath.ElementAt(i);
                summedDistance += Vector2.Distance(lastWpPos, wp.Position);
                float timeNeeded = (summedDistance/(Speed*60));
                //TraceLog.Write("distance to travel: " + summedDistance);

                if (timeNeeded > time)
                {
                    float timeOverhead = time - timeSpend;
                    float distanceNeeded = Vector2.Distance(lastWpPos, wp.Position);
                    float distancefraction = ((timeOverhead*Speed)*60);
                    t = distancefraction/distanceNeeded;
                    break;
                }

                index++;
                timeSpend = timeNeeded;
                lastWpPos.X = wp.Position.X;
                lastWpPos.Y = wp.Position.Y;
            }

            CurrentIndex = index;

            // get the interpolated position
            return GetPlayersPosition(GetWaypoint(index - 1).Position, GetWaypoint(index).Position, t);

        }

        /// <summary>
        /// Make sure we do not override the array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetClampedIndex(int index)
        {
            if (index < 0)
                index = 0;

            if (index > CurrentWalkpath.Count - 1)
                index = CurrentWalkpath.Count - 1;
           
            return index;
        }

        /// <summary>
        /// return a valid way point
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Waypoint GetWaypoint(int index)
        {
            return CurrentWalkpath.ElementAt(GetClampedIndex(index));
        }

        #endregion Private
    }
}