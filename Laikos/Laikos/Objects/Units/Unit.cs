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
        public bool walk, idle, attack;
        public UnitType type;
        public double HP;
        public double maxHP;
        public bool budowniczy = false;
        TimeSpan timeSpan;
        Player Player;

        //////////////////////////////////
        // PathFiding Variables
        //////////////////////////////////
        public ZnajdzSciezke pathFiding;
        public List<Wspolrzedne> destinyPoints;
        public IEnumerator<Wspolrzedne> destinyPointer;
        public BoundingSphere boundingSphere;
        Vector3 direction;

        public Unit()
            : base()
        {
            walk = false;
        }

        public Unit(Game game, Player player, UnitType type, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3))
            : base(game, type.model)
        {
            this.Player = player;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.type = (UnitType)type.Clone();
            if (this.type.name.Contains("Worker"))
                budowniczy = true;

            maxHP = this.type.maxhp;
            HP = maxHP;
            this.pathFiding = new ZnajdzSciezke();
            this.pathFiding.mapaUstaw();
        }

        public void Update(GameTime gameTime)
        {

            if (walk)
            {
                this.currentModel.player.PlayClip("Walk", true);
                walk = false;
            }

            if (idle)
            {
                this.currentModel.player.PlayClip("Idle", true);
                idle = false;
            }

            if (attack)
            {
                this.currentModel.player.PlayClip("Attack", true);
                attack = false;
            }


            HandleEvent(gameTime);
            HP = (int)MathHelper.Clamp((float)HP, 0, (float)maxHP);
            this.CleanMessages();
            base.Update(gameTime);


        }

        public override void HandleEvent(GameTime gameTime)
        {

            EventManager.FindMessagesByDestination(this, messages);
            FindDoubledMessages();


            //for (int i = 0; i < 0; i++)
            if(messages.Count > 0)
            {
                int i = 0;
                if (messages[i].Done == false)
                switch (messages[i].Type)
                {
                    case (int)EventManager.Events.Selected:
                        selected = true;
                        messages[i].Done = true;
                        break;

                    case (int)EventManager.Events.Unselected:
                        selected = false;
                        messages[i].Done = true;
                        break;

                    case (int)EventManager.Events.Interaction:

                        Console.WriteLine("Jednostka - interakcja z" + messages[i].Destination.ToString());
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, this, messages[i].Payload, null));
                        messages[i].Done = true;
                        break;

                    case (int)EventManager.Events.MoveUnit:
                        
                            if ((destinyPoints != null) && (destinyPoints.Count > 0) && (destinyPointer == null))
                            {
                                destinyPointer = destinyPoints.GetEnumerator();
                                destinyPointer.MoveNext();
                                Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                direction = vecTmp - Position;
                            }

                            direction.Normalize();

                            Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                            Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;

                            if ((destinyPointer != null) && (Math.Abs(Position.X - destinyPointer.Current.X) < 0.5f) && (Math.Abs(Position.Z - destinyPointer.Current.Y) < 0.5f))
                            {
                                
                                // Next step walk.
                                if (!destinyPointer.MoveNext())
                                {
                                    destinyPoints = null;
                                    destinyPointer = null;
                                    
                                    direction.X = 0.0f;
                                    direction.Z = 0.0f;

                                    messages[i].Done = true;
                                    idle = true;
                                    Console.WriteLine("done");
                                }
                                else
                                {
                                    Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                    direction = vecTmp - Position;
                                }
                            }
                        
                        break;

                    case (int)EventManager.Events.MoveToBuild:
                        
                        if (budowniczy == true)
                        {
                           
                            if ((destinyPoints != null) && (destinyPoints.Count > 0) && (destinyPointer == null))
                            {
                                destinyPointer = destinyPoints.GetEnumerator();
                                destinyPointer.MoveNext();
                                Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                direction = vecTmp - Position;
                            }

                            direction.Normalize();

                            Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                            Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                           
                            if ((destinyPointer != null) && (Math.Abs(Position.X - destinyPointer.Current.X) < 0.5f) && (Math.Abs(Position.Z - destinyPointer.Current.Y) < 0.5f))
                            {
                                // Next step walk. 
                                
                                if (!destinyPointer.MoveNext())
                                {
                                    destinyPoints = null;
                                    destinyPointer = null;

                                    direction.X = 0.0f;
                                    direction.Z = 0.0f;

                                    messages[i].Done = true;
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Build, this, this, messages[i].Payload));
                                    timeSpan = TimeSpan.FromMilliseconds(Player.BuildingTypes["Cementary"].buildtime); //czas budowania zmienic
                                    attack = true;
                                }
                                else
                                {
                                    Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                    direction = vecTmp - Position;
                                }
                            }
    
                        }

                        break;

                    case (int)EventManager.Events.Build:

                        if (budowniczy == true)
                        {
                           timeSpan -= gameTime.ElapsedGameTime;
                            if (timeSpan < TimeSpan.Zero)
                            {
                                Player.Build(Player.BuildingTypes["Cementary"], (Vector3)(messages[i].Payload));
                                messages[i].Done = true;
                                idle = true;
                            }   
                        }
                        break;
                }
            }
        }
    }
}
