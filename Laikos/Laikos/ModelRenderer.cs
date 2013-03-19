using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using SkinnedModel;

namespace Laikos
{
    class ModelRenderer
    {
        public AnimationPlayer animationPlayer; //Controls the animation, references a method in the pre-loaded project
        public AnimationClip clip; //Contains the animation clip currently playing
        public Model currentModel; //Model reference
        public SkinningData skinningData; //Used by the AnimationPlayer method
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 Rotation = new Vector3(0, 0, 0); //Current rotation
        Terrain terrain;
        public float Scale = 1.0f; //Current scale
        SpriteFont font;
        SpriteBatch batch;
        public ModelRenderer(Model currentModelInput, Terrain t, SpriteBatch batch, ContentManager content)
        {
            currentModel = currentModelInput;
            // Look up our custom skinning information.
            skinningData = currentModel.Tag as SkinningData;
            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");
            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);
            terrain = t;
            font = content.Load<SpriteFont>("Verdana");
            this.batch = batch;
        }
        public void Update(GameTime gameTime)
        {
            AddGravity();
            CheckCollisionWithTerrain();
            HandleInput();
            if ((clip != null)) //ensure that the animation currently playing by the method will be played in sequence and in time with everything else using the passed 'gameTime'
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
        }
        public void Draw(Camera camera)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] *= 
                    Matrix.CreateRotationX(Rotation.X) //Computes the rotation
                    * Matrix.CreateRotationY(Rotation.Y)
                    * Matrix.CreateRotationZ(Rotation.Z)
                    * Matrix.CreateScale(Scale) //Applys the scale
                    * Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up); //Move the models position
            }

            //Ask camera for matrix.
            Matrix view = camera.viewMatrix;
 
            //Ask for 3D projection for this model
            Matrix projection = camera.projectionMatrix;
 
            //Render the skinned mesh.
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
            clip = skinningData.AnimationClips[Animation];
            if (clip != animationPlayer.CurrentClip)
                animationPlayer.StartClip(clip);
        }

        private void AddGravity()
        {
            Position.Y -= 0.1f;
        }

        private void CheckCollisionWithTerrain()
        {
            float terrainHeight = terrain.GetExactHeightAt(Position.X, Position.Z);
            float x = 0.1f;

            if (Position.Y < terrainHeight + x)
            {
                Vector3 newPos = Position;
                newPos.Y = (terrainHeight + x);
                Position = newPos;
            }
        }

        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                Position.Z += 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                Position.Z -= 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                Position.X -= 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                Position.X += 0.1f;
            }
        }
    }
}
