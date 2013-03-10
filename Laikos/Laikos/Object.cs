using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Laikos
{
    class Object
    {
        public Model Model;
        KeyboardState oldState;
        public Vector3 Position = Vector3.Zero; //pozycja obiektu
        public Vector3 Velocity = Vector3.Zero;//predkosc obiektu
        public Vector3 VelocityAdd = Vector3.Zero;
        public float RotationAngle { get; set; } //obrot w okolo osi z (jak juz zmieni sie kamera)


        public void LoadContent(ContentManager content)
        {
            Model = content.Load<Model>("carrot");
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            VelocityAdd = Vector3.Zero;
            RotationAngle = 0;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in Model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index]
                        * Matrix.CreateRotationY(RotationAngle)
                        * Matrix.CreateTranslation(Position);
                    effect.View = view;
                    effect.Projection = projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }

        public void Update(GameTime gameTime)
        {
             HandleInput();

             VelocityAdd.X = (float)Math.Sin(RotationAngle);
             VelocityAdd.Z = (float)Math.Cos(RotationAngle);

             Velocity += VelocityAdd * 0.1f;

             Position += Velocity; //dodaje velocity
             Velocity *= 0.95f; //velocity zmniejsz
             if (Velocity.LengthSquared() < 0.01f)
                Velocity = Vector3.Zero;
        }

        private void HandleInput()
        {
            KeyboardState newState = Keyboard.GetState();
            //velocity on right click
            if (newState.IsKeyDown(Keys.L))
                RotationAngle -= 0.01f;
            if (newState.IsKeyDown(Keys.J))
                RotationAngle += 0.01f;

            // if I'm not pressing forwards then no velocity
            if (newState.IsKeyDown(Keys.I))
            {
            }
            else if (newState.IsKeyDown(Keys.K))
            {
                VelocityAdd *= -1;
            }
            else
            {
                VelocityAdd = Vector3.Zero;
            }

            oldState = newState;

            // Check to see whether the Spacebar is down.
            //    if (newState.IsKeyDown(Keys.Space))
            //   {
            // Key has just been pressed.
            //   }
            // Otherwise, check to see whether it was down before.
            // (and therefore just released)
            //   else if (oldState.IsKeyDown(Keys.Space))
            //{
            // Key has just been released.
            //}
        }
    }

}
