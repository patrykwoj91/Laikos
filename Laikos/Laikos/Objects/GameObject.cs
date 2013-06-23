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

        public float Rotation_Y_Add
        {
            set
            {
                Rotation.Y += value;

                if (MathHelper.ToDegrees(Rotation.Y) > 180.0f)
                {
                    Rotation.Y -= MathHelper.ToRadians(360.0f);
                }
                else if (MathHelper.ToDegrees(Rotation.Y) < -180.0f)
                {
                    Rotation.Y += MathHelper.ToRadians(360.0f);
                }
            }
        }

        public float Scale = 1.0f; //Current scale
        public AnimatedModel currentModel = null; //model
        bool exists = false;
        public float temp_position = 0.0f;
        public Player player;
        public Game game;

        public bool selected = false;
        public List<Message> messages;

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
            this.messages = new List<Message>();
        }

        public GameObject(Game game, Player player, String path)
        {
            this.player = player;
            this.messages = new List<Message>();

            this.game = game;
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

        protected void CleanMessages()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].Done == true)
                {
                    Console.WriteLine("Usuwam " + (EventManager.Events)messages[i].Type);
                    messages.RemoveAt(i);
                }
            }
        }

        protected void FindDoubledMessages()
        {
            for (int i = 0; i < messages.Count - 1; i++)
                for (int j = i + 1; j < messages.Count; j++)
                {
                    if (messages[i].CompareTo(messages[j]) == 0)
                    {
                        if (messages[i].time.CompareTo(messages[j].time) > 0)
                            messages[j].Done = true;
                        else
                            messages[i].Done = true;
                    }
                }
        }
    }
}
