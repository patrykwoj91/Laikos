using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
   public class GameObject
    {
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 lastPosition = new Vector3(0, 0, 0);
        public Vector3 Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(0), MathHelper.ToRadians(0)); //Current rotation
        public float Scale = 1.0f; //Current scale
        public AnimatedModel currentModel = null; //model
        
        public bool selected = false;
       
        
        public Matrix GetWorldMatrix()
        {
            return
                Matrix.CreateScale(Scale) *
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public GameObject()
        {    
        }

        public GameObject(Game game, String path)
            
        {
            currentModel = new AnimatedModel(path);
            currentModel.LoadContent(game.Content);

        }

        public void Update(GameTime gameTime)
        { 
            currentModel.Update(gameTime);
            Collisions.AddGravity(ref Position);
            Collisions.CheckWithTerrain(ref Position, 0.5f);
        }

        public virtual void HandleEvent(GameTime gameTime)
        {
        }
    }
}
