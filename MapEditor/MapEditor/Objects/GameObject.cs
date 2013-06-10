using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;
using Microsoft.Xna.Framework.Storage;
using System.IO;


namespace Laikos
{
   public class GameObject
    {
        public Vector3 Position = new Vector3(0, 0, 0); //Model current position on the screen
        public Vector3 lastPosition = new Vector3(0, 0, 0);
        public Vector3 Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(0), MathHelper.ToRadians(0)); //Current rotation
        public float Scale = 1.0f; //Current scale
        public AnimatedModel currentModel = null; //model
        public AnimatedModel High = null;
        public AnimatedModel Mid = null;
        public AnimatedModel Low = null;
        bool exists = false;
        public float temp_position = 0.0f;

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

            if ((File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/" + path + "High" + ".xnb"))) &&
                (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/" + path + "Mid" + ".xnb"))) &&
                (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/" + path + "Low" + ".xnb"))))
            {
                High = new AnimatedModel(path + "High");
                High.LoadContent(game.Content);
                Mid = new AnimatedModel(path + "Mid");
                Mid.LoadContent(game.Content);
                Low = new AnimatedModel(path + "Low");
                Low.LoadContent(game.Content);
                currentModel = Mid;
                temp_position = currentModel.player.current_Position;
                exists = true;
            }
            else
            {
                currentModel = new AnimatedModel(path);
                currentModel.LoadContent(game.Content);
            }
        }

        public void Update(GameTime gameTime)
        {
            
            if (exists)
            {
                if (Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) + 
                    Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) + 
                    Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)) < 30)
                {
                    temp_position = currentModel.player.current_Position;
                    currentModel = High;
                    currentModel.player.current_Position = temp_position;
                   // Console.WriteLine("High: " + Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) +
                   // Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) +
                   // Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)));
                }
                else if (Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) +
                    Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) +
                    Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)) > 30
                    && Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) +
                    Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) +
                    Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)) < 50)
                {
                    temp_position = currentModel.player.current_Position;
                    currentModel = Mid;
                    currentModel.player.current_Position = temp_position;
                   // Console.WriteLine("Mid: " + Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) +
                   // Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) +
                   // Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)));
                }
                else
                {
                    temp_position = currentModel.player.current_Position;
                    currentModel = Low;
                    currentModel.player.current_Position = temp_position;
                   // Console.WriteLine("Low: " + Math.Sqrt(Math.Pow(this.Position.X - Camera.cameraPosition.X, 2) +
                   // Math.Pow(this.Position.Y - Camera.cameraPosition.Y, 2) +
                   // Math.Pow(this.Position.Z - Camera.cameraPosition.Z, 2)));
                }
            }

            currentModel.Update(gameTime);
            Collisions.AddGravity(ref Position);
            Collisions.CheckWithTerrain(ref Position, 0.5f);
        }

        public virtual void HandleEvent(GameTime gameTime)
        {
        }
    }
}
