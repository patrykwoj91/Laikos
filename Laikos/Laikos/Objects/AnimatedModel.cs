using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Animation;

namespace Laikos
{
    /// <summary>
    /// An encloser for an XNA model that we will use that includes support for
    /// bones, animation, and some manipulations.
    /// </summary>
    public class AnimatedModel
    {
        #region Fields

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        public Model model = null;

        /// <summary>
        /// Extra data associated with the XNA model fe lists of clips
        /// </summary>
        private ModelExtra modelExtra = null;

        /// <summary>
        /// The model bones
        /// </summary>
        private List<Bone> bones = new List<Bone>();

        /// <summary>
        /// The model asset name
        /// </summary>
        private string assetName = "";

        /// <summary>
        /// An associated animation clip player
        /// </summary>
        public AnimationPlayer player;

        private Effect GBuffer;
        private Texture2D normals;
        private Texture2D speculars;
        #endregion

        #region Properties

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        public Model Model
        {
            get { return model; }
        }

        /// <summary>
        /// The underlying bones for the model
        /// </summary>
        public List<Bone> Bones { get { return bones; } }

        /// <summary>
        /// The model animation clips
        /// </summary>
        public Dictionary<string,AnimationClip> Clips { get { return modelExtra.Clips; } }

        #endregion

        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates the model from an XNA model
        /// </summary>
        /// <param name="assetName">The name of the asset for this model</param>
        public AnimatedModel(string assetName)
        {
            this.assetName = assetName;

        }

        /// <summary>
        /// Load the model asset from content
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
    
            this.model = content.Load<Model>(assetName);
            modelExtra = model.Tag as ModelExtra;

            GBuffer = content.Load<Effect>("Effects/GBUffer");
            normals = content.Load<Texture2D>("null_normal");
            speculars = content.Load<Texture2D>("null_specular");

            ObtainBones();
            player = new AnimationPlayer(Clips, this);
        }


        #endregion

        #region Bones Management

        /// <summary>
        /// Get the bones from the model and create a bone class object for
        /// each bone. We use our bone class to do the real animated bone work.
        /// </summary>
        private void ObtainBones()
        {
            bones.Clear();
            foreach (ModelBone bone in model.Bones)
            {
                // Create the bone object and add to the heirarchy
                Bone newBone = new Bone(bone.Name, bone.Transform, bone.Parent != null ? bones[bone.Parent.Index] : null);

                // Add to the bones for this model
                bones.Add(newBone);
            }
        }

        /// <summary>
        /// Find a bone in this model by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Bone FindBone(string name)
        {
            foreach(Bone bone in Bones)
            {
                if (bone.Name == name)
                    return bone;
            }

            return null;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Update animation for the model.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (player != null)
            {
                player.Update(gameTime);
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw the model
        /// </summary>
        /// <param name="graphics">The graphics device to draw on</param>
        /// <param name="camera">A camera to determine the view</param>
        /// <param name="world">A world matrix to place the model</param>
        public void Draw(GraphicsDevice device, Matrix world)
        {
            if (model == null)
                return;

            //
            // Compute all of the bone absolute transforms
            //

            Matrix[] boneTransforms = new Matrix[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                Bone bone = bones[i];
                bone.ComputeAbsoluteTransform();

                boneTransforms[i] = bone.AbsoluteTransform;
            }

            //
            // Determine the skin transforms from the skeleton
            //

            Matrix[] skeleton = new Matrix[modelExtra.Skeleton.Count];
            for (int s = 0; s < modelExtra.Skeleton.Count; s++)
            {
                Bone bone = bones[modelExtra.Skeleton[s]];
                skeleton[s] = bone.SkinTransform * bone.AbsoluteTransform;
            }

            //Ask camera for matrix.
            Matrix view = Camera.viewMatrix;

            //Ask for 3D projection for this model
            Matrix projection = Camera.projectionMatrix;


            GBuffer.CurrentTechnique = GBuffer.Techniques["NoSkinning"];
            GBuffer.Parameters["View"].SetValue(Camera.viewMatrix);
            GBuffer.Parameters["Projection"].SetValue(Camera.projectionMatrix);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    device.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    device.Indices = part.IndexBuffer;
                    
                    GBuffer.Parameters["World"].SetValue(boneTransforms[mesh.ParentBone.Index] * world);
                    GBuffer.Parameters["Texture"].SetValue(part.Effect.Parameters["Texture"].GetValueTexture2D());
                    GBuffer.Parameters["NormalMap"].SetValue(normals);
                    GBuffer.Parameters["SpecularMap"].SetValue(speculars);
                    GBuffer.Parameters["Bones"].SetValue(skeleton);
                    GBuffer.CurrentTechnique.Passes[0].Apply();

                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }
        #endregion

    }
}
