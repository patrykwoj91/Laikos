using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
namespace Laikos
{
    public class Unit : GameObject
    {
        public bool walk, picked;
        public List<Message> messages;

        public Unit()
            : base()
        {
            walk = false;
            picked = false;
            this.messages = new List<Message>();
        }

        public Unit(Game game, string path, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3))
            : base(game, path)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.messages = new List<Message>();
        }

        public void Update(GameTime gameTime)
        {
            EventManager.FindMessageByDestination(this, messages);
            //Console.WriteLine(messages.Count);
            for (int i = 0; i < messages.Count; i++ )
            {
                switch (messages[i].Type)
                {
                    case (int)EventManager.Events.ScaleUp:
                        Scale = 0.07f;
                        break;
                    case (int)EventManager.Events.ScaleDown:
                        Scale = 0.1f;
                        break;
                } 
            }
            Input.HandleUnit(ref walk, ref lastPosition, ref Position, ref Rotation, picked);
            messages.Clear();
            base.Update(gameTime);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            base.Draw(graphics);
        }
    }
}
