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

            public Matrix GetWorldMatrix()
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

            public Decoration(Model currentModelInput)
            {
                currentModel = currentModelInput;
                Position = new Vector3(30, 0, 150);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
                Scale = 0.1f;
                Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(0), MathHelper.ToRadians(0));
            }

            public virtual void Update(GameTime gameTime)
            {
                Collisions.CheckWithTerrain(ref Position, 0.5f);
            }

            public virtual void Draw()
            {
                //Ask camera for matrix.
                Matrix view = Camera.viewMatrix;

                //Ask for 3D projection for this model
                Matrix projection = Camera.projectionMatrix;

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
            
            
            public bool checkIfPossible(Vector3 startPosition)
            {
                BoundingBox box = XNAUtils.TransformBoundingBox((BoundingBox)currentModel.Tag, GetWorldMatrix());
                Vector3 size = box.Max - box.Min;
                float lowestPoint = float.MaxValue;
                float highestPoint = float.MinValue;
                
                for (int i = (int)startPosition.X; i < size.X + startPosition.X; i++)
                {
                    for (int j = (int)startPosition.Z; j < size.Z + startPosition.Z; j++)
                    {
                        if (Terrain.GetClippedHeightAt(i, j) < lowestPoint) lowestPoint = Terrain.GetClippedHeightAt(i, j);
                        if (Terrain.GetClippedHeightAt(i, j) > lowestPoint) highestPoint = Terrain.GetClippedHeightAt(i, j);
                    }
                }

                Console.WriteLine(lowestPoint + " " + highestPoint);
                if (highestPoint - lowestPoint < 1)
                    return true;
                else
                    return false;
            }
    }
    
}
