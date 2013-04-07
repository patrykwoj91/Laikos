using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Animation;

namespace Laikos
{
    class Decoration
    {
            public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
            public Vector3 Rotation = new Vector3(0, 0, 0); //Current rotation
            public float Scale = 1.0f; //Current scale
            public Model currentModel; //Model reference

            Terrain terrain;

            private Matrix GetWorldMatrix()
            {
                return
                    Matrix.CreateScale(Scale) *
                    Matrix.CreateRotationX(Rotation.X) *
                    Matrix.CreateRotationY(Rotation.Y) *
                    Matrix.CreateRotationZ(Rotation.Z) *
                    Matrix.CreateTranslation(Position);
            }

            public Decoration()
            {
            }

            public Decoration(Model currentModelInput, Terrain terrain)
            {
                currentModel = currentModelInput;
                Position = new Vector3(30, 50, 150);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
                Scale = 0.1f;
                Rotation = new Vector3(MathHelper.ToRadians(-90), MathHelper.ToRadians(0), MathHelper.ToRadians(0));
                this.terrain = terrain;
            }
            public virtual void Update(GameTime gameTime)
            {
                AddGravity();
                CheckCollisionWithTerrain();
            }
            public virtual void Draw(Camera camera)
            {
                //Ask camera for matrix.
                Matrix view = camera.viewMatrix;

                //Ask for 3D projection for this model
                Matrix projection = camera.projectionMatrix;

                // Render the skinned mesh.
                foreach (ModelMesh mesh in currentModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = view;
                        effect.World = GetWorldMatrix();
                        effect.Projection = projection;
                        effect.EnableDefaultLighting();
                    }

                    mesh.Draw();
                }
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


    }
    
}
