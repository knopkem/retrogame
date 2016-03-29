using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameEngine.TileEngine
{
    /// <summary>
    /// Controls the object using the proper method for the platform
    /// </summary>
    public class InputHandler
    {
        public void TakeControlOfObject(InputState input, int playerIndex, ref ControllableObject obj)
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DoubleTap | GestureType.Tap;
            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            var target = new Vector2(0, 0);

#if WINDOWS_PHONE
            foreach (GestureSample sample in input.Gestures)
            {
                switch (sample.GestureType)
                {
                    case GestureType.FreeDrag:
                        obj.Position += sample.Delta;
                        obj.Target += sample.Delta;
                        break;
                }
            }
#else

            // XBOX
            if (gamePadState.ThumbSticks.Left.Length() > 0.2f)
            {
                target = ((gamePadState.ThumbSticks.Left*new Vector2(1, -1f))*50.0f);
            }
            else
            {
                // WINDOWS
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    target += new Vector2(-1, 0);
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    target += new Vector2(1, 0);
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    target += new Vector2(0, -1);
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    target += new Vector2(0, 1);
                }
                if ((target.X != 0) || (target.Y != 0))
                    target.Normalize();
                target.X *= obj.Speed;
                target.Y *= obj.Speed;
            }

            if ((target.X != 0) || (target.Y != 0))
                obj.Target = obj.Position + target;
            else
                obj.Target = obj.Position;
#endif
        }
    }
}