using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
    class Decoration
    {
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 lastPosition = new Vector3(0, 0, 0);
        public Vector3 Rotation = new Vector3(0, 0, 0); //Current rotation
        public float Scale = 1.5f; //Current scale
        public AnimatedModel currentModel = null; //model
        public AnimationPlayer player;



        public Matrix GetWorldMatrix()
        {
            return
                Matrix.CreateScale(Scale) *
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public Decoration(Game game, String path)
        {
            //tu ustawiamy rozne cuda

            //Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            Position = new Vector3(40, 50, 150);
            lastPosition = Position;
            Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(0), MathHelper.ToRadians(0));

            currentModel = new AnimatedModel(path);
            currentModel.LoadContent(game.Content);




            // And play the clip
            player = currentModel.PlayClip(currentModel.Clips["Take 001"]);
            player.Looping = true;
        }

        public void Update(GameTime gameTime)
        {

            currentModel.Update(gameTime);
            Collisions.AddGravity(ref Position);
            Collisions.CheckWithTerrain(ref Position, 1f);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            currentModel.Draw(graphics.GraphicsDevice, GetWorldMatrix());
        }
            
            
        public bool checkIfPossible(Vector3 startPosition)
        {
            BoundingBox box = XNAUtils.TransformBoundingBox(Collisions.GetBoundingBox(currentModel.Model), GetWorldMatrix());
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