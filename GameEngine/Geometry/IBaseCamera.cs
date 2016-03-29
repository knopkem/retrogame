using GameEngine.TileEngine;
using Microsoft.Xna.Framework;

namespace GameEngine.Geometry
{
    public interface IBaseCamera
    {
        Matrix ProjectionMatrix { get; }
        Matrix ViewMatrix { get; }

        void Update(GameTime gametime);

        void FocusOnObject(ControllableObject _obj);
    }
}