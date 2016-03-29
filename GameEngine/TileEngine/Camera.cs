using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledLib;

namespace GameEngine.TileEngine
{
    public class Camera2D
    {
        public Rectangle        ClampRect;
        public float            Height;
        public float            Width;
        public Vector2          Position;
        public Vector2          Target;

        private ControllableObject _lockObject;
        private const float Speed = 0.2f;
        
        
        /// <summary>
        /// Initialize the camera, using the game map to define the boundaries
        /// </summary>
        /// <param name="vp">Graphics viewport</param>
        /// <param name="map">Game Map</param>
        public Camera2D(Viewport vp, Map map)
            : this(vp, new Rectangle(0, 0, (map.Width*map.TileWidth), (map.Height*map.TileHeight)))
        {
        }

        /// <summary>
        /// Initialize the camera, using a custom rectangle to define the boundaries
        /// </summary>
        /// <param name="vp">Graphics viewport</param>
        /// <param name="clampRect">A rectangle defining the map boundaries, in pixels</param>
        public Camera2D(Viewport vp, Rectangle clampRect)
        {
            Position = new Vector2(0, 0);
            Width = vp.Width;
            Height = vp.Height;

            ClampRect = clampRect;

            // Set initial position and target
            Position.X = ClampRect.X;
            Position.Y = ClampRect.Y;
            Target = new Vector2(ClampRect.X, ClampRect.Y);
        }

        /// <summary>
        /// Set an object that the camera will use to focus on, assign null to focus on the Target vector instead
        /// </summary>
        /// <param name="obj"></param>
        public void FocusOnObject(ControllableObject obj)
        {
            _lockObject = obj;
        }

        /// <summary>
        /// create a matrix for the camera to offset everything we draw, the map and our objects. since the
        /// camera coordinates are where the camera is, we offset everything by the negative of that to simulate
        /// a camera moving. we also cast to integers to avoid filtering artifacts
        /// </summary>
        /// <param name="scale">scale factor in case of zooming</param>
        /// <returns>Matrix</returns>
        public Matrix GetCameraMatrix(float scale)
        {
            Matrix cameraMatrix = Matrix.CreateTranslation(-(int)this.Position.X, -(int)this.Position.Y, 0);
            cameraMatrix *= Matrix.CreateScale(1 / scale, 1 / scale, 1);
            return cameraMatrix;
        }

        /// <summary>
        /// Update the camera
        /// </summary>
        public void Update()
        {
            // update the position of the object to follow
            if (_lockObject != null)
            {
                Target = _lockObject.Position - new Vector2(Width/2, Height/2);
            }

            // Clamp target to map/camera bounds
            Target.X = (int) Math.Floor(MathHelper.Clamp(Target.X, ClampRect.X, ClampRect.Width - Width));
            Target.Y = (int) Math.Floor(MathHelper.Clamp(Target.Y, ClampRect.Y, ClampRect.Height - Height));

            // Move camera toward target
            Position = Vector2.SmoothStep(Position, Target, Speed);
        }
    }
}