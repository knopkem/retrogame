using System;
using GameEngine.ScreenManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace RetroGame
{
    public class HudEventArgs : EventArgs
    {
        public int ButtonType;
    }

    /// <summary>
    /// Draws a Heads Up Display onto the screen and sends events that can be used to trigger things
    /// </summary>
    internal class HudDisplay
    {
        #region fields

        public Rectangle ViewRectangle;

        private readonly CharPool _charPool;
        private readonly ScreenManager _screenManager;
        private Point _hudSize;
        private int _lineWidth;
        private SharedData _sharedData;
        private SpriteFont _spriteFont;
        private Texture2D _spritePoint;


        #endregion

        #region props

        public Point HudSize { get; set; }

        #endregion

        public HudDisplay(ScreenManager manager)
        {
            _screenManager = manager;

            // we load the singleton at construction
            _sharedData = GameManager.Instance.Data;
            _charPool = _sharedData.GlobalCharPool;
            ViewRectangle = _screenManager.GraphicsDevice.Viewport.Bounds;
        }

        public event EventHandler<HudEventArgs> ItemSelected;


        public void LoadContent(ContentManager content)
        {
            _hudSize.X = 1280;
            _hudSize.Y = 80;
            _lineWidth = 2;

            HudSize = new Point((int) (_hudSize.X*_screenManager.DrawingScale.X),
                                (int) (_hudSize.Y*_screenManager.DrawingScale.Y));
            _spritePoint = content.Load<Texture2D>("OtherTextures/whiteblock");
            _spriteFont = content.Load<SpriteFont>("SpriteFonts/menufont");
        }

        public void Update(GameTime gameTime, InputState input)
        {
            foreach (GestureSample sample in input.Gestures)
            {
                switch (sample.GestureType)
                {
                    case GestureType.Tap:

                        var tapLocation = new Point((int) sample.Position.X, (int) sample.Position.Y);

                        if (ViewRectangle.Contains(tapLocation))
                        {
                            var arg = new HudEventArgs();
                            arg.ButtonType = 0;
                            // check if we tapped on the photo
                            tapLocation.X = (int) (tapLocation.X/_screenManager.drawingScale.X);
                            tapLocation.Y = (int) (tapLocation.Y/_screenManager.drawingScale.Y);
                            Rectangle photoRec = _charPool.GetCurrentCharacter().PhotoRectangle;
                            if (photoRec.Contains(tapLocation))
                                arg.ButtonType = 1;
                            ItemSelected(this, arg);
                        }
                        break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the background and the delimiting line
            spriteBatch.Draw(_spritePoint, new Rectangle(0, 0, _hudSize.X, _hudSize.Y - _lineWidth), Color.DarkGray);
            spriteBatch.Draw(_spritePoint, new Rectangle(0, _hudSize.Y - _lineWidth, _hudSize.X, _lineWidth), Color.Black);

            // draw the character photo + name
            Character2D currentPlayer = _charPool.GetCurrentCharacter();
            spriteBatch.DrawString(_spriteFont, currentPlayer.CharProps.CharName, new Vector2(20, 60), Color.Black, 0,
                                   Vector2.Zero, 0.6f, SpriteEffects.None, 0);
            currentPlayer.DrawPhoto(new Vector2(20, 10), spriteBatch);

            // draw the current object beneath the cursor
            MapObjectExtended obj = _sharedData.GlobalMapObjectManager.GetObjectAtPosition(currentPlayer.Cursor.Position);
            spriteBatch.DrawString(_spriteFont, CommonHelper.EObjectToName(obj.ObjectType),
                                   new Vector2(200, 60), Color.Black, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
        }


    }
}