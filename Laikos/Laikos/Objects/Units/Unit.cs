using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using MyDataTypes;
namespace Laikos
{
    public class Unit : GameObject
    {
        public bool walk;
        public List<Message> messages;
        public UnitType type;

        public Unit()
            : base()
        {
            walk = false;
            this.messages = new List<Message>();
        }

        public Unit(Game game, UnitType type, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3))
            : base(game, type.model)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.messages = new List<Message>();
            this.type = (UnitType)type.Clone();
        }

        public void Update(GameTime gameTime)
        {
            HandleEvent(gameTime);
            base.Update(gameTime);
        }

        public override void HandleEvent(GameTime gameTime)
        {
            EventManager.FindMessagesByDestination(this, messages);
            // Console.WriteLine(messages.Count); 
            for (int i = 0; i < messages.Count; i++)
            {
                switch (messages[i].Type)
                {
                    case (int)EventManager.Events.Selected:
                        selected = true;
                        break;

                    case (int)EventManager.Events.Unselected:
                        selected = false;
                        break;

                    case (int)EventManager.Events.Interaction:

                        Console.WriteLine("Jednostka - interakcja z" + messages[i].Payload.ToString());
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, this, messages[i].Payload, null));
                        break;

                    case (int)EventManager.Events.MoveUnit: //nakladajace sie komunikaty powoduja problem z kolejnymi ruchami 
                        if (selected)
                        {
                            Vector3 direction = (Vector3)messages[i].Payload - Position;
                            direction.Normalize();

                            Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 500;
                            Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 500;
                            if (Math.Abs(Position.X - ((Vector3)messages[i].Payload).X) < 0.5f && Math.Abs(Position.Z - ((Vector3)messages[i].Payload).Z) < 0.5f)
                            {

                            }
                        }
                        break;
                }
            }
        }

    }
}
