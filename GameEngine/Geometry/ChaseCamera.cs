#region File Description

//-----------------------------------------------------------------------------
// ChaseCamera.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework;

#endregion

namespace GameEngine.Geometry
{
    /// <remarks>This class was first seen in the Chase Camera sample.</remarks>
    public class ChaseCamera
    {
        #region Chased object properties (set externally each frame)

        private Vector3 up = Vector3.Up;

        /// <summary>
        /// Position of object being chased.
        /// </summary>
        public Vector3 ChasePosition { get; set; }

        /// <summary>
        /// Direction the chased object is facing.
        /// </summary>
        public Vector3 ChaseDirection { get; set; }

        /// <summary>
        /// Chased object's Up vector.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        #endregion

        #region Desired camera positioning (set when creating camera or changing view)

        private Vector3 desiredPosition;
        private Vector3 desiredPositionOffset = new Vector3(0, 2.0f, 2.0f);
        private Vector3 lookAt;
        private Vector3 lookAtOffset = new Vector3(0, 2.8f, 0);

        /// <summary>
        /// Desired camera position in the chased object's coordinate system.
        /// </summary>
        public Vector3 DesiredPositionOffset
        {
            get { return desiredPositionOffset; }
            set { desiredPositionOffset = value; }
        }

        /// <summary>
        /// Desired camera position in world space.
        /// </summary>
        public Vector3 DesiredPosition
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();

                return desiredPosition;
            }
        }

        /// <summary>
        /// Look at point in the chased object's coordinate system.
        /// </summary>
        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }

        /// <summary>
        /// Look at point in world space.
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                // Ensure correct value even if update has not been called this frame
                UpdateWorldPositions();

                return lookAt;
            }
        }

        #endregion

        #region Camera physics (typically set when creating camera)

        private float damping = 600.0f;
        private float mass = 50.0f;
        private float stiffness = 1800.0f;

        /// <summary>
        /// Physics coefficient which controls the influence of the camera's position
        /// over the spring force. The stiffer the spring, the closer it will stay to
        /// the chased object.
        /// </summary>
        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
        }

        /// <summary>
        /// Physics coefficient which approximates internal friction of the spring.
        /// Sufficient damping will prevent the spring from oscillating infinitely.
        /// </summary>
        public float Damping
        {
            get { return damping; }
            set { damping = value; }
        }

        /// <summary>
        /// Mass of the camera body. Heaver objects require stiffer springs with less
        /// damping to move at the same rate as lighter objects.
        /// </summary>
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        #endregion

        #region Current camera properties (updated by camera physics)

        private Vector3 collisionPosition;
        private Vector3 position;

        private Vector3 velocity;

        /// <summary>
        /// Position of camera in world space.
        /// </summary>
        public Vector3 Position
        {
            get { return collisionPosition; }
        }

        /// <summary>
        /// Velocity of camera.
        /// </summary>
        public Vector3 Velocity
        {
            get { return velocity; }
        }

        #endregion

        #region Matrix properties

        private Matrix view;

        /// <summary>
        /// View transform matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = ChasePosition +
                              Vector3.TransformNormal(DesiredPositionOffset, transform);
            lookAt = ChasePosition +
                     Vector3.TransformNormal(LookAtOffset, transform);
        }

        /// <summary>
        /// Rebuilds camera's view and projection matrices.
        /// </summary>
        private void UpdateMatrices()
        {
            view = Matrix.CreateLookAt(Position, LookAt, Up);
        }

        /// <summary>
        /// Forces camera to be at desired position and to stop moving. The is useful
        /// when the chased object is first created or after it has been teleported.
        /// Failing to call this after a large change to the chased object's position
        /// will result in the camera quickly flying across the world.
        /// </summary>
        public void Reset()
        {
            UpdateWorldPositions();

            // Stop motion
            velocity = Vector3.Zero;

            // Force desired position
            position = desiredPosition;

            UpdateMatrices();
        }

        /// <summary>
        /// Animates the camera from its current position towards the desired offset
        /// behind the chased object. The camera's animation is controlled by a simple
        /// physical spring attached to the camera and anchored to the desired position.
        /// </summary>
        public void Update(float elapsedTime)
        {
            UpdateWorldPositions();

            // Calculate spring force
            Vector3 stretch = position - desiredPosition;
            Vector3 force = -stiffness*stretch - damping*velocity;

            // Apply acceleration
            Vector3 acceleration = force/mass;
            velocity += acceleration*elapsedTime;

            // Apply velocity
            position += velocity*elapsedTime;
            collisionPosition = position;

            UpdateMatrices();
        }

        #endregion
    }
}