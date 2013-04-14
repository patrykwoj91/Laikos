using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
    class GameUnit
    {
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 lastPosition = new Vector3(0, 0, 0);
        public Vector3 Rotation = new Vector3(0, 0, 0); //Current rotation
        public float Scale = 1.0f; //Current scale
        public AnimatedModel currentModel = null; //model
        public AnimationClip clip = new AnimationClip(); //switch betweent clips
        public bool walk,picked;

        public Matrix GetWorldMatrix()
        {
            return
                Matrix.CreateScale(Scale) *
                Matrix.CreateRotationX(Rotation.X) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateTranslation(Position);
        }

        public GameUnit(Game game, String path)
            
        {
            //tu ustawiamy rozne cuda
  
            //Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            Position = new Vector3(0, 50, 33);
            lastPosition = Position;
            Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(180), MathHelper.ToRadians(0));

            Scale = 0.05f;
            walk = false;
            picked = false;

            currentModel = new AnimatedModel(path);
            currentModel.LoadContent(game.Content);

            clip = currentModel.Clips["Idle"];

            // And play the clip
            AnimationPlayer player = currentModel.PlayClip(clip);
            player.Looping = true;
        }

        public void Update(GameTime gameTime)
        {
            if (walk)
            {
                clip = currentModel.Clips["Walk"];
            }
            else
            {
                clip = currentModel.Clips["Idle"];
            }

            currentModel.Update(gameTime);

            

            Collisions.AddGravity(ref Position);
            Input.HandleUnit(ref walk, ref lastPosition, ref Position, ref Rotation, picked);
            Collisions.CheckWithTerrain(ref Position, 0.5f);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            currentModel.Draw(graphics.GraphicsDevice, GetWorldMatrix());
        }
    }
}
