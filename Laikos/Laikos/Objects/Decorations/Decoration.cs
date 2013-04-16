using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
   public class Decoration : GameObject
    {
       public Message Message = null;

       public Decoration()
           :base()
       {
       }

       public Decoration(Game game, string path, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3), Message message = null)
            : base(game, path)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Message = message;
        }

        public void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            base.Draw(graphics);
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