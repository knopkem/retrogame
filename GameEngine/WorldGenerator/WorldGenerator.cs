using GameEngine.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledLib;

namespace GameEngine.WorldGenerator
{
    /// <summary>
    /// Generate 3d world from map.
    /// Creates walls and objects and places them in the scene
    /// </summary>
    public class WorldGenerator
    {
        #region fields
        
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;
        
        private readonly CubePrimitive _cube;
        private readonly TileLayer _tileLayer;
        private readonly Map _tileMap;
        private readonly float _aspectRatio;
        private IBaseCamera _camera;
        private const int Height = 137;
        private const int Width = 117;
        private int _mapHeight;
        private int _mapWidth;

        #endregion

        public WorldGenerator(Game game, GraphicsDevice graphicsDevice, Map map)
        {
            _cube = new CubePrimitive(graphicsDevice);
            _tileMap = map;
            _tileLayer = _tileMap.GetLayer("Walls") as TileLayer;
            _aspectRatio = graphicsDevice.Viewport.AspectRatio;

            _mapWidth = Width*_tileMap.TileWidth;
            _mapHeight = Height*_tileMap.TileHeight;

            _camera = null;
        }

        public float AspectRatio
        {
            get { return _aspectRatio; }
        }

        public void SetCamera(IBaseCamera camera)
        {
            this._camera = camera;
        }

        public void Update(GameTime gameTime)
        {
            // handle camera
            if (_camera == null) return;
            _camera.Update(gameTime);
            ViewMatrix = _camera.ViewMatrix;
            ProjectionMatrix = _camera.ProjectionMatrix;
        }

        public void Draw()
        {
            // draw the map build from cubes
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    // position all cubes
                    Matrix world = Matrix.CreateTranslation(new Vector3(x, y, 0));
                    
                    // check if there is a wall
                    if (_tileLayer.Tiles[x, y] == null) continue;

                    // scale those that are walls and color them white;
                    world *= Matrix.CreateScale(_tileMap.TileWidth, _tileMap.TileWidth,
                                                (float) (8.0*_tileMap.TileWidth));
                    _cube.Draw(world, ViewMatrix, ProjectionMatrix, Color.White);
                }
        }
    }
}