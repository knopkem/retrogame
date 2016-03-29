using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Geometry
{
    /// <summary>
    /// Base class for all model based objects
    /// </summary>
    internal class PositionedModel : PositionedObject
    {
        #region Fields

        // Cache information about the model size and position.
        private readonly Model model;
        private Matrix[] boneTransforms;
        private Vector3 modelCenter;
        private float modelRadius;

        #endregion

        public PositionedModel(Model myModel)
        {
            model = myModel;
        }

        private void MeasureModel()
        {
            // Look up the absolute bone transforms for this model.
            boneTransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Compute an (approximate) model center position by
            // averaging the center of each mesh bounding sphere.
            modelCenter = Vector3.Zero;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshBounds = mesh.BoundingSphere;
                Matrix transform = boneTransforms[mesh.ParentBone.Index];
                Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

                modelCenter += meshCenter;
            }

            modelCenter /= model.Meshes.Count;

            // Now we know the center point, we can compute the model radius
            // by examining the radius of each mesh bounding sphere.
            modelRadius = 0;

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshBounds = mesh.BoundingSphere;
                Matrix transform = boneTransforms[mesh.ParentBone.Index];
                Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

                float transformScale = transform.Forward.Length();

                float meshRadius = (meshCenter - modelCenter).Length() +
                                   (meshBounds.Radius*transformScale);

                modelRadius = Math.Max(modelRadius, meshRadius);
            }
        }
    }
}