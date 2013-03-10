using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;

namespace Laikos
{
    class Object
    {
        AnimationPlayer animationPlayer;
        Model model;

        public Vector3 Position = new Vector3(0, 50, -150); //position
        public float RotationAngle { get; set; } //facingdirection

         private Matrix GetWorldMatrix()
         {
             return
                 Matrix.CreateScale(1) *
                 Matrix.CreateRotationX(MathHelper.ToRadians(180)) *
                 Matrix.CreateRotationY(RotationAngle) *
                 Matrix.CreateRotationY(MathHelper.ToRadians(0)) *
                 Matrix.CreateTranslation(Position);
         }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("dude");
            // Look up our custom skinning information.
            SkinningData skinningData = model.Tag as SkinningData;
            
            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];
            animationPlayer.StartClip(clip);
        }

        public void Draw(Matrix view, Matrix projection, Matrix world)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            // Render the skinned mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }

                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }

        public void Update(GameTime gameTime)
        {

             animationPlayer.Update(gameTime.ElapsedGameTime, true, GetWorldMatrix());

             SkinningData skinningData = model.Tag as SkinningData;

             HandleInput(skinningData);
            
        }

        private void HandleInput(SkinningData skinningData)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                AnimationClip clip = skinningData.AnimationClips["greet"];
                animationPlayer.StartClip(clip);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                AnimationClip clip = skinningData.AnimationClips["stand"];
                animationPlayer.StartClip(clip);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D3))
            {
                AnimationClip clip = skinningData.AnimationClips["jump"];
                animationPlayer.StartClip(clip);
            }
        }
    }

}
