using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Animation;

namespace Laikos
{
    class Unit
    {
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 Rotation = new Vector3(0, 0, 0); //Current rotation
        public float Scale = 1.0f; //Current scale
        public AnimationPlayer animationPlayer; //Controls the animation, references a method in the pre-loaded project
        public AnimationData animationData; //Used by the AnimationPlayer method
        public AnimationClip clip; //Contains the animation clip currently playing
        public Model currentModel; //Model reference

        public Matrix GetWorldMatrix()
        {
            return
                Matrix.CreateScale(Scale) *
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public Unit()
        {
        }

        public Unit(Model currentModelInput)
        {
            currentModel = currentModelInput;
            // Look up our custom skinning information.
            animationData = currentModel.Tag as AnimationData;
            if (animationData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");
            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(animationData);

        }
        public virtual void Update(GameTime gameTime)
        {
            if ((clip != null))
            animationPlayer.Update(gameTime.ElapsedGameTime, true, GetWorldMatrix());
        }
        public virtual void Draw()
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            //Ask camera for matrix.
            Matrix view = Camera.viewMatrix;

            //Ask for 3D projection for this model
            Matrix projection = Camera.projectionMatrix;

            // Render the skinned mesh.
            foreach (ModelMesh mesh in currentModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }
        
        public void PlayAnimation(String Animation)
        {

            clip = animationData.AnimationClips[Animation];
            if (clip != animationPlayer.CurrentClip)
                animationPlayer.StartClip(clip);
        }
    }
}




       
       