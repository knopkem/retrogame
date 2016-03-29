using System;
using GameEngine.TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RetroGameData;
using SkinnedModelData;

namespace RetroGame
{
    /// <summary>
    /// Special character that represents a 3d model
    /// </summary>
    internal class Character3D : Character2D
    {
        #region fields

        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;
        public Matrix WorldMatrix;

        private AnimationPlayer _animationPlayer;
        private Model _charModel;

        #endregion

        #region props

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="map"></param>
        /// <param name="props"></param>
        public Character3D(MapHelper map, CharProperties props) : base(map, props)
        {
        }

        public Character3D(Character2D cloneFrom) : base(cloneFrom.MapHelperObj, cloneFrom.CharProps)
        {
            Position = cloneFrom.Position;
            TaskList = cloneFrom.TaskList;
            CharProps = cloneFrom.CharProps;
        }

        #region private functions

        private Matrix GetFaceDirectionMatrix()
        {
            Vector2 faceDirection = base.GetFaceDirection();

            return Matrix.CreateRotationZ(VectorToAngle(faceDirection));
        }

        private static float VectorToAngle(Vector2 vector)
        {
            return (float) Math.Atan2(vector.X, -vector.Y);
        }

        #endregion

        #region public functions

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            _charModel = content.Load<Model>("Models/dude");

            // Look up our custom skinning information.
            var skinningData = _charModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            _animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            _animationPlayer.StartClip(clip);
        }

        public override void Update(GameTime gameTime, Camera2D gameCamera)
        {
            _animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            base.Update(gameTime, gameCamera);
        }

        public void Draw()
        {
            var worldPosition = new Vector3(Position.X, Position.Y, 0);


            Matrix[] bones = _animationPlayer.GetSkinTransforms();

            WorldMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(90))*
                          Matrix.CreateRotationZ(MathHelper.ToRadians(180))*GetFaceDirectionMatrix()*
                          Matrix.CreateScale(1.6f)*Matrix.CreateTranslation(worldPosition);

            // Render the skinned mesh.
            foreach (ModelMesh mesh in _charModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = ViewMatrix;
                    effect.Projection = ProjectionMatrix;
                    effect.World = bones[mesh.ParentBone.Index]*WorldMatrix;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }

        }

        #endregion
    }
}