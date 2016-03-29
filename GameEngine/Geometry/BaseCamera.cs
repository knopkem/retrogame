using System;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Geometry
{
    /// <summary>
    /// Manages the camera the player will use
    /// </summary>
    public class BaseCamera : GameComponent, IBaseCamera
    {
        private readonly Vector3    _upVector;
        private float               _aspectRatio;
        private Vector3             _lookAt;
        private Vector3             _position;
        private Matrix              _projectionMatrix;
        private float               _rotationAngle;
        private Vector3             _rotationAxis;
        private Matrix              _rotationMatrix;
        private Matrix              _viewMatrix;
        private ControllableObject  _lockObject;

        #region Properties

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 LookAt
        {
            get { return _lookAt; }
            set { _lookAt = value; }
        }

        public Viewport ViewPort
        {
            set { _aspectRatio = (value.Width)/((float) value.Height); }
            get { throw new NotImplementedException(); }
        }

        public Matrix ViewMatrix
        {
            get { return _viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return _projectionMatrix; }
        }

        #endregion

        #region public methods

        public BaseCamera(Game game) : base(game)
        {
            var cam = (IBaseCamera) game.Services.GetService(typeof (IBaseCamera));
            if (cam == null)
                game.Services.AddService(typeof (IBaseCamera), this);

            _upVector = Vector3.Up;

            _rotationAxis = Vector3.Up;
            _rotationAngle = 0.0f;

            _rotationMatrix = Matrix.Identity;
        }

        public override void Update(GameTime gameTime)
        {
            // sync camera with object
            if (_lockObject != null)
            {
                Position = new Vector3(_lockObject.Position.X, _lockObject.Position.Y, 500);
                LookAt = new Vector3(_lockObject.Position.X, _lockObject.Position.Y, 0);
            }

            _rotationMatrix = Matrix.CreateFromAxisAngle(_rotationAxis, _rotationAngle);

            Vector3 rotatedPosition = Vector3.Transform(_position, _rotationMatrix);
            Vector3 rotatedUpVector = Vector3.Transform(_upVector, _rotationMatrix);

            _viewMatrix = Matrix.CreateLookAt(rotatedPosition, _lookAt, rotatedUpVector);
        }

        public void FocusOnObject(ControllableObject obj)
        {
            _lockObject = obj;
        }

        public override void Initialize()
        {
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40.0f),
                                                                    _aspectRatio,
                                                                    1.0f,
                                                                    10000.0f);
            base.Initialize();
        }

        /// <summary>
        /// Rotates the camera around a given vector
        /// </summary>
        public void RotateCameraAroundTarget(Vector3 axis, float angle)
        {
            _rotationAxis = axis;
            _rotationAngle = angle;
        }

        #endregion
    }
}