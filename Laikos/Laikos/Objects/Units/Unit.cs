using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using MyDataTypes;

using Laikos.PathFiding;

namespace Laikos
{
    public class Unit : GameObject
    {
        public bool walk;
        public List<Message> messages;
        public UnitType type;
        public double HP;
        public double maxHP;

        //////////////////////////////////
        // PathFiding Variables
        //////////////////////////////////
        public ZnajdzSciezke pathFiding;
        public List<Wspolrzedne> destinyPoints;
        public IEnumerator<Wspolrzedne> destinyPointer;
        Vector3 direction;


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
            maxHP = this.type.maxhp;
            HP = maxHP;

            this.pathFiding = new ZnajdzSciezke();
            this.pathFiding.mapaUstaw();
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
                            if ((destinyPoints != null) && (destinyPoints.Count > 0) && (destinyPointer == null))
                            {
                                destinyPointer = destinyPoints.GetEnumerator();
                                destinyPointer.MoveNext();
                                Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                direction = vecTmp - Position;
                            }

                            direction.Normalize();

                            Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 30;
                            Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 30;

                            //Console.WriteLine(Position.X + " " + destinyPointer.Current.X + ", " + Position.Z + " " + destinyPointer.Current.Y);

                            if ((destinyPointer != null) && (Math.Abs(Position.X - destinyPointer.Current.X) < 0.5f) && (Math.Abs(Position.Z - destinyPointer.Current.Y) < 0.5f))
                            {
                                // Next step walk.
                                if (!destinyPointer.MoveNext())
                                {
                                    destinyPoints = null;
                                    destinyPointer = null;

                                    direction.X = 0;
                                    direction.Z = 0;
                                }
                                else
                                {
                                    Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                    direction = vecTmp - Position;
                                }
                            }
                        }
                        break;
                }
            }
        }

    }
}
