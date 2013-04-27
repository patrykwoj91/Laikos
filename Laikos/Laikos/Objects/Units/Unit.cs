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
        public int messageSent = 0;
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
                        Scale = 0.1f;
                        break;
                    case (int)EventManager.Events.ScaleDown:
                        Scale = 0.07f;
                        break;
                    case (int)EventManager.Events.Selected:
                        picked = true;
                        //messages.Clear();
                        //EventManager.ClearMessages();
                        break;
                    case (int)EventManager.Events.Unselected:
                        picked = false;
                        //messages.Clear();
                        //EventManager.ClearMessages();
                        break;
                    case (int)EventManager.Events.PickBox:
                        if (Collisions.GeneralDecorationCollisionCheck(messages[i].Destination, messages[i].Sender) && picked == true)
                        {
                            if (messages[i].Sender.currentModel.Model.Meshes[0].Name == "Chest_TreasureChest")
                            {
                                if (messageSent < 1)
                                {
                                    Console.WriteLine("Podniesiono skarb");
                                    messageSent++;
                                }
                                EventManager.CreateMessage(new Message((int)EventManager.Events.MoveUnit, null, messages[i].Destination, new Vector3(5, Terrain.GetExactHeightAt(5, 30), 50)));
                            }
                        }
                        break;
                    case (int)EventManager.Events.MoveUnit:
                        if (picked == true)
                        {
                            Vector3 direction = (Vector3)messages[i].Payload - Position;
                            direction.Normalize();

                            Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 500;
                            Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 500;
                            if (Math.Abs(Position.X - ((Vector3)messages[i].Payload).X) < 0.5f && Math.Abs(Position.Z - ((Vector3)messages[i].Payload).Z) < 0.5f)
                            {
                                messages.Clear();
                                EventManager.ClearMessages();
                            }
                        }
                        break;
                } 
            }
            Input.HandleUnit(ref walk, ref lastPosition, ref Position, ref Rotation, picked,player,currentModel);
            messages = new List<Message>();
            base.Update(gameTime);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            base.Draw(graphics);
        }
    }
}
