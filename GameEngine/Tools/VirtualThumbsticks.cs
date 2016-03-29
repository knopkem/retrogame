#region File Description

//-----------------------------------------------------------------------------
// VirtualThumbsticks.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace GameEngine.Tools
{
    /// <summary>
    /// Represents virtual thumbsticks which allow touch input.
    /// Users can touch the left half of the screen to place the center
    /// of the left thumbstick and the right half for the right thumbstick. 
    /// Users can then drag away from that center to simulate thumbstick input.
    ///
    /// This is a static class with static methods to get the thumbstick properties, 
    /// for consistency with other XNA input
    /// classes like TouchPanel, Gamepad, Keyboard, etc.
    /// </summary>
    public static class VirtualThumbsticks
    {
        #region Fields

        // the distance in screen pixels that represents a thumbstick value of 1f.
        private const float MaxThumbstickDistance = 60f;

        // the current positions of the physical touches
        private static Vector2 _leftPosition;
        private static Vector2 _rightPosition;

        // the IDs of the touches we are tracking for the thumbsticks
        private static int _leftId = -1;
        private static int _rightId = -1;

        /// <summary>
        /// Gets the center position of the left thumbstick.
        /// </summary>
        public static Vector2? LeftThumbstickCenter { get; private set; }

        /// <summary>
        /// Gets the center position of the right thumbstick.
        /// </summary>
        public static Vector2? RightThumbstickCenter { get; private set; }

        #endregion

        /// <summary>
        /// Gets the value of the left thumbstick.
        /// </summary>
        public static Vector2 LeftThumbstick
        {
            get
            {
                // if there is no left thumbstick center, return a value of (0, 0)
                if (!LeftThumbstickCenter.HasValue)
                {
                    return Vector2.Zero;
                }

                // calculate the scaled vector from the touch position to the center,
                // scaled by the maximum thumbstick distance
                Vector2 l = (_leftPosition - LeftThumbstickCenter.Value)/MaxThumbstickDistance;

                // if the length is more than 1, normalize the vector
                if (l.LengthSquared() > 1f)
                {
                    l.Normalize();
                }

                return l;
            }
        }

        /// <summary>
        /// Gets the value of the right thumbstick.
        /// </summary>
        public static Vector2 RightThumbstick
        {
            get
            {
                // if there is no left thumbstick center, return a value of (0, 0)
                if (!RightThumbstickCenter.HasValue)
                {
                    return Vector2.Zero;
                }

                // calculate the scaled vector from the touch position to the center,
                // scaled by the maximum thumbstick distance
                Vector2 r = (_rightPosition - RightThumbstickCenter.Value)/MaxThumbstickDistance;

                // if the length is more than 1, normalize the vector
                if (r.LengthSquared() > 1f)
                {
                    r.Normalize();
                }

                return r;
            }
        }

        /// <summary>
        /// Updates the virtual thumbsticks based on current touch state. This must be called every frame.
        /// </summary>
        public static void Update(InputState input)
        {
            TouchLocation? leftTouch = null;
            TouchLocation? rightTouch = null;
            TouchCollection touches = input.TouchState;

            // Examine all the touches to convert them to virtual dpad positions. Note that the 'touches'
            // collection is the set of all touches at this instant, not a sequence of events. The only
            // sequential information we have access to is the previous location for of each touch.
            foreach (TouchLocation touch in touches)
            {
                if (touch.Id == _leftId)
                {
                    // This is a motion of a left-stick touch that we're already tracking
                    leftTouch = touch;
                    continue;
                }

                if (touch.Id == _rightId)
                {
                    // This is a motion of a right-stick touch that we're already tracking
                    rightTouch = touch;
                    continue;
                }

                // We didn't continue an existing thumbstick gesture; see if we can start a new one.
                //
                // We'll use the previous touch position if possible, to get as close as possible to where
                // the gesture actually began.
                TouchLocation earliestTouch;
                if (!touch.TryGetPreviousLocation(out earliestTouch))
                {
                    earliestTouch = touch;
                }

                if (_leftId == -1)
                {
                    // if we are not currently tracking a left thumbstick and this touch is on the left
                    // half of the screen, start tracking this touch as our left stick
                    if (earliestTouch.Position.X < TouchPanel.DisplayWidth/2)
                    {
                        leftTouch = earliestTouch;
                        continue;
                    }
                }

                if (_rightId == -1)
                {
                    // if we are not currently tracking a right thumbstick and this touch is on the right
                    // half of the screen, start tracking this touch as our right stick
                    if (earliestTouch.Position.X >= TouchPanel.DisplayWidth/2)
                    {
                        rightTouch = earliestTouch;
                        continue;
                    }
                }
            }

            // if we have a left touch
            if (leftTouch.HasValue)
            {
                // if we have no center, this position is our center
                if (!LeftThumbstickCenter.HasValue)
                {
                    LeftThumbstickCenter = leftTouch.Value.Position;
                }

                // save the position of the touch
                _leftPosition = leftTouch.Value.Position;

                // save the ID of the touch
                _leftId = leftTouch.Value.Id;
            }
            else
            {
                // otherwise reset our values to not track any touches
                // for the left thumbstick
                LeftThumbstickCenter = null;
                _leftId = -1;
            }

            // if we have a right touch
            if (rightTouch.HasValue)
            {
                // if we have no center, this position is our center
                if (!RightThumbstickCenter.HasValue)
                {
                    RightThumbstickCenter = rightTouch.Value.Position;
                }

                // save the position of the touch
                _rightPosition = rightTouch.Value.Position;

                // save the ID of the touch
                _rightId = rightTouch.Value.Id;
            }
            else
            {
                // otherwise reset our values to not track any touches
                // for the right thumbstick
                RightThumbstickCenter = null;
                _rightId = -1;
            }
        }
    }
}