﻿using System;
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
                Position = new Vector3(30, 50, 150);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
                Scale = 0.1f;
                Rotation = new Vector3(MathHelper.ToRadians(-90), MathHelper.ToRadians(0), MathHelper.ToRadians(0));
            }

            public virtual void Update(GameTime gameTime)
            {
                Collisions.AddGravity(ref Position);
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
    }
    
}