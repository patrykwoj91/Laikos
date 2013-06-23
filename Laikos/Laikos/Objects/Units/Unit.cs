﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using MyDataTypes;

using Laikos.PathFiding;
using Microsoft.Xna.Framework.Audio;

namespace Laikos
{
    public class Unit : GameObject
    {
        public UnitType type;
        public double HP;
        public double maxHP;
        int Souls_owned;
        const int Souls_cap = 50;
        public bool budowniczy = false;
        public TimeSpan timeSpan;

        //////////////////////////////////
        // PathFiding Variables
        //////////////////////////////////
        public ZnajdzSciezke pathFiding;
        public List<Wspolrzedne> destinyPoints;
        public IEnumerator<Wspolrzedne> destinyPointer;
        public BoundingSphere boundingSphere;
        Vector3 direction;

        //////////////////////////////////
        // Fight Variables
        //////////////////////////////////
        public int damage;
        public int range;
        public int ratio;

        private double lastHP;

        Unit destinyUnit;
        Building destinyBuilding;

        public bool dead = false;

        public Unit()
            : base()
        {
        }

        public Unit(Game game, Player player, UnitType type, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3))
            : base(game, player, type.model)
        {
            
            Souls_owned = 0;
            this.player = player;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.type = (UnitType)type.Clone();

            if (this.type.name.Contains("Worker"))
            {
                this.budowniczy = true;
            }

            this.maxHP = this.type.maxhp;
            this.HP = this.maxHP;
            this.lastHP = this.HP;
            setIdle();
            this.damage = this.type.damage;
            this.range = this.type.range;
            this.ratio = this.type.ratio;

            this.pathFiding = new ZnajdzSciezke();
            this.pathFiding.mapaUstaw();

        }

        public void Update(GameTime gameTime)
        {
            if (HP <= 0)
            {
                dead = true;
                messages.Clear();
            }
            //Counterattack
            else if (this.HP < this.lastHP)
            {
                this.lastHP = this.HP;

                bool none = true;
                foreach (Message _msg in messages)
                {
                    if 
                    (
                        (
                            (_msg.Type == (int)EventManager.Events.Attack)
                            ||
                            (_msg.Type == (int)EventManager.Events.MoveToAttack)
                        )
                        && 
                        (!_msg.Done))
                    {
                        none = false;
                        break;
                    }
                }

                if (none)
                {
                    Unit _uni = FindEnemy();
                    if (_uni != null)
                    {
                        
                        EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, this, _uni));
                    }
                }
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
            if (messages.Count > 0)
            {
                int i = 0;
                if (!messages[i].Done)
                    switch (messages[i].Type)
                    {
                        #region HandleEvent.Selected/Unselected
                        case (int)EventManager.Events.Selected:
                            selected = true;
                            messages[i].Done = true;
                            GUI.CreateMessage(new Message((int)EventManager.Events.GuiUP, this, null, null));
                            break;

                        case (int)EventManager.Events.Unselected:
                            selected = false;
                            messages[i].Done = true;
                            GUI.CreateMessage(new Message((int)EventManager.Events.GuiDOWN, this, null, null));
                            break;
                        #endregion

                        #region HandleEvent.Interaction
                        case (int)EventManager.Events.Interaction:

                            Console.WriteLine("Jednostka - interakcja z" + messages[i].Destination.ToString());
                            EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, this, messages[i].Payload, null));
                            messages[i].Done = true;
                            break;
                        #endregion

                        #region HandleEvent.MoveUnit
                        case (int)EventManager.Events.MoveUnit:
                            //////nowa wersja////////
                            if (destinyPoints == null)
                            {
                                setWalk();
                                Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)this.Position.X, (int)this.Position.Z);
                                Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)((Vector3)messages[i].Payload).X, (int)(((Vector3)messages[i].Payload).Z));

                                this.destinyPoints = this.pathFiding.obliczSciezke(wspBegin, wspEnd);
                                this.destinyPointer = null;
                            }
                            /////////nowa wersja///////////

                            if (destinyPoints.Count > 0)
                            {
                                if ((destinyPoints != null) && (destinyPoints.Count > 0) && (destinyPointer == null))
                                {
                                    destinyPointer = destinyPoints.GetEnumerator();
                                    destinyPointer.MoveNext();
                                    Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                    direction = vecTmp - Position;
                                }

                                if
                                (
                                    (!Double.IsNaN(direction.X)) &&
                                    (!Double.IsNaN(direction.Y)) &&
                                    (!Double.IsNaN(direction.Z))
                                )
                                {
                                    direction.Normalize();

                                    Rotation.Y = ComputeRadian(new Vector2(Position.X, Position.Z), new Vector2(direction.X, direction.Z));

                                    Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                    Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                }

                                if ((destinyPointer != null) && (Math.Abs(Position.X - destinyPointer.Current.X) < 0.5f) && (Math.Abs(Position.Z - destinyPointer.Current.Y) < 0.5f))
                                {

                                    // Next step walk.
                                    if (!destinyPointer.MoveNext())
                                    {
                                        EndMove(messages[i]);

                                        setIdle();
                                        Console.WriteLine("done");
                                    }
                                    else
                                    {
                                        Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                        direction = vecTmp - Position;
                                    }
                                }
                            }
                            else
                            {
                                messages[i].Done = true;
                            }

                            break;
                        #endregion

                        #region HandleEvent.MoveToBuild
                        case (int)EventManager.Events.MoveToBuild:

                            if (EventManager.MessageToOld(gameTime, messages[i], 4000))
                            {
                                messages[i].Done = true;
                                break;
                            }
                            if (budowniczy == true)
                            {
                                //////nowa wersja////////
                                if (destinyPoints == null)
                                {
                                    Vector3 stay_here = new Vector3(((WhereToBuild)messages[i].Payload).position.X, ((WhereToBuild)messages[i].Payload).position.Y, ((WhereToBuild)messages[i].Payload).position.Z);

                                    if (MathUtils.RandomNumber(1, 2) == 1) //czy x czy Z
                                    {
                                        //if (MathUtils.RandomNumber(1, 2) == 1) // czy + czy -
                                        //      stay_here.X += BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f;

                                        //else
                                        //     stay_here.X -= BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f;

                                        //stay_here.Z += MathUtils.RandomNumber((int)(stay_here.Z - BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius - unit.boundingSphere.Radius - 0.1f),
                                        //    (int)(stay_here.Z + BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f));
                                    }
                                    else
                                    {
                                        // if (MathUtils.RandomNumber(1, 2) == 1) // czy + czy -
                                        //    stay_here.Z += BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f;
                                        //else
                                        //    stay_here.Z -= BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f;

                                        //stay_here.X += MathUtils.RandomNumber((int)(stay_here.Z - BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius - unit.boundingSphere.Radius - 0.1f),
                                        // (int)(stay_here.Z + BoundingSphere.CreateFromBoundingBox(temp.boundingBox).Radius + unit.boundingSphere.Radius + 0.1f));
                                    }

                                    setWalk();
                                    Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)this.Position.X, (int)this.Position.Z);
                                    Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)((WhereToBuild)messages[i].Payload).position.X, (int)(((WhereToBuild)messages[i].Payload).position.Z));

                                    this.destinyPoints = this.pathFiding.obliczSciezke(wspBegin, wspEnd);
                                    this.destinyPointer = null;
                                }
                                /////////nowa wersja///////////
                                if (destinyPoints.Count > 0)
                                {

                                    if ((destinyPoints != null) && (destinyPoints.Count > 0) && (destinyPointer == null))
                                    {
                                        destinyPointer = destinyPoints.GetEnumerator();
                                        destinyPointer.MoveNext();
                                        Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                        direction = vecTmp - Position;
                                    }

                                    if
                                    (
                                        (!Double.IsNaN(direction.X)) &&
                                        (!Double.IsNaN(direction.Y)) &&
                                        (!Double.IsNaN(direction.Z))
                                    )
                                    {
                                        direction.Normalize();

                                        Rotation.Y = ComputeRadian(new Vector2(Position.X, Position.Z), new Vector2(direction.X, direction.Z));

                                        Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                        Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                    }

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

                                            timeSpan = TimeSpan.FromMilliseconds(((WhereToBuild)messages[i].Payload).building.buildtime);
                                            setAttack();
                                        }
                                        else
                                        {
                                            Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                            direction = vecTmp - Position;
                                        }
                                    }
                                }
                                else
                                {
                                    messages[i].Done = true;
                                }
                            }
                            messages[i].timer = gameTime.TotalGameTime;
                            break;
                        #endregion

                        #region HandleEvent.Build
                        case (int)EventManager.Events.Build:
                            if (EventManager.MessageToOld(gameTime, messages[i], 100))
                            {
                                messages[i].Done = true;
                                break;
                            }
                            if (budowniczy == true)
                            {
                                timeSpan -= gameTime.ElapsedGameTime;
                                if (timeSpan < TimeSpan.Zero)
                                {
                                    player.Build(((WhereToBuild)messages[i].Payload).building.type, ((WhereToBuild)messages[i].Payload).position);
                                    messages[i].Done = true;
                                    setIdle();
                                }
                            }
                            messages[i].timer = gameTime.TotalGameTime;
                            break;
                        #endregion

                        #region HandleEvent.Gathering
                        case (int)EventManager.Events.Gather:
                            ///////BREAK MESSAGE
                            if (EventManager.MessageToOld(gameTime, messages[i], 3000))
                            {
                                messages[i].Done = true;
                                break;
                            }
                            /////////////////////

                            if (budowniczy == true)
                            {
                                Vector3 poczatek_ruchu = Vector3.Zero;
                                //////////MOVE
                                if (destinyPoints == null)
                                {
                                    setWalk();
                                    Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)this.Position.X, (int)this.Position.Z);
                                    Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)((Building)messages[i].Sender).Position.X, (int)((Building)messages[i].Sender).Position.Z);
                                    poczatek_ruchu.X = this.Position.X;
                                    poczatek_ruchu.Z = this.Position.Z;
                                    this.destinyPoints = this.pathFiding.obliczSciezke(wspBegin, wspEnd);
                                    this.destinyPointer = null;
                                }
                                if (this.destinyPoints.Count > 0)
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

                                        //JESTES U CELU
                                        if (!destinyPointer.MoveNext())
                                        {

                                            destinyPoints = null;
                                            destinyPointer = null;
                                            direction.X = 0.0f;
                                            direction.Z = 0.0f;
                                            //idle = true;

                                            //OBSŁUGA ZBIERANIA
                                            timeSpan -= gameTime.ElapsedGameTime;
                                            if (((Building)messages[i].Sender).Souls > 0) //czy budynek ma narobione dusze
                                            {
                                                if (this.Souls_owned < Souls_cap) //czy mamy miejsce
                                                {

                                                    if (timeSpan < TimeSpan.Zero) //jesli czas pakowania minal dodajemy dusze
                                                    {
                                                        if (Souls_cap - this.Souls_owned >= ((Building)messages[i].Sender).Souls) //ilosc miejsca wieksza niz ilosc dusz w budynku
                                                        {
                                                            this.Souls_owned += ((Building)messages[i].Sender).Souls; //dodajemy wszystkie dusze z budynku
                                                            ((Building)messages[i].Sender).Souls = 0;
                                                        }
                                                        else //ilosc miejsca mniejsza niz ilosc dusz w budynku
                                                        {
                                                            ((Building)messages[i].Sender).Souls -= Souls_cap - this.Souls_owned; //dobieramy tyle ile sie da
                                                            this.Souls_owned += Souls_cap - this.Souls_owned;
                                                        }
                                                        foreach (Building building in player.BuildingList)
                                                        {
                                                            if (building.type.Name.Contains("Pałac rady")) //odsylamy do skladowania
                                                            {
                                                                EventManager.CreateMessage(new Message((int)EventManager.Events.Store, messages[i].Sender, this, building.Position));
                                                                messages[i].Done = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                else //jak nie mamy miejsca to odnosimy
                                                {
                                                    foreach (Building building in player.BuildingList)
                                                    {
                                                        if (building.type.Name.Contains("Pałac rady")) //odsylamy do skladowania
                                                        {
                                                            EventManager.CreateMessage(new Message((int)EventManager.Events.Store, this, this, building.Position));
                                                            messages[i].Done = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else //jak nie ma dusz to koniec zadania
                                            {
                                                messages[i].Done = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                            direction = vecTmp - Position;
                                        }
                                    }
                                    setWalk();
                                }
                                else
                                {
                                    messages[i].Done = true;
                                }
                            }
                            messages[i].timer = gameTime.TotalGameTime;
                            break;
                        #endregion

                        #region HandleEvent.Storing
                        case (int)EventManager.Events.Store:
                            ///////BREAK MESSAGE
                            if (EventManager.MessageToOld(gameTime, messages[i], 3000))
                            {
                                messages[i].Done = true;
                                break;
                            }
                            /////////////////////

                            if (budowniczy == true)
                            {
                                //////////MOVE
                                if (destinyPoints == null)
                                {
                                    setWalk();
                                    Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)this.Position.X, (int)this.Position.Z);
                                    Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)((Vector3)messages[i].Payload).X, (int)((Vector3)messages[i].Payload).Z);

                                    this.destinyPoints = this.pathFiding.obliczSciezke(wspBegin, wspEnd);
                                    this.destinyPointer = null;
                                }
                                if (this.destinyPoints.Count > 0)
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

                                        //JESTES U CELU
                                        if (!destinyPointer.MoveNext())
                                        {
                                            destinyPoints = null;
                                            destinyPointer = null;
                                            direction.X = 0.0f;
                                            direction.Z = 0.0f;
                                            //OBSŁUGA ZBIERANIA
                                            player.Souls += this.Souls_owned;
                                            this.Souls_owned = 0;
                                            EventManager.CreateMessage(new Message((int)EventManager.Events.Gather, messages[i].Sender, this, null));
                                     
                                            timeSpan = TimeSpan.FromMilliseconds(3000);
                                            messages[i].Done = true;
                                            break;
                                        }
                                        else
                                        {
                                            Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                            direction = vecTmp - Position;
                                        }
                                    }
                                    setWalk();
                                }
                                else
                                {
                                    messages[i].Done = true;
                                }
                            }
                            messages[i].timer = gameTime.TotalGameTime;
                            break;
                        #endregion

                        #region Move To Attack
                        case (int)EventManager.Events.MoveToAttack:

                            GameObject targetMoveAttack = (GameObject)messages[i].Payload;

                            if (destinyPoints == null)
                            {
                                setWalk();
                                Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)this.Position.X, (int)this.Position.Z);
                                Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)(targetMoveAttack.Position.X), (int)(targetMoveAttack.Position.Z));

                                this.destinyPoints = this.pathFiding.obliczSciezke(wspBegin, wspEnd);
                                this.destinyPointer = null;
                            }

                            if (messages[i].Payload is Unit)
                            {
                                destinyUnit = (Unit)messages[i].Payload;
                                destinyBuilding = null;
                            }
                            else if (messages[i].Payload is Building)
                            {
                                destinyUnit = null;
                                destinyBuilding = (Building)messages[i].Payload;
                            }

                            if ((destinyPoints != null) && (destinyPoints.Count > 0))
                            {
                                if
                                (
                                    (
                                        (destinyUnit != null) 
                                        &&
                                        (Math.Abs(Position.X - destinyUnit.Position.X) < range)
                                    )
                                    || 
                                    (
                                        (destinyBuilding != null)
                                        &&
                                        (Math.Abs(Position.Z - destinyBuilding.Position.Z) < range)
                                    )
                                )
                                {
                                    EndMove(messages[i]);

                                    setAttack();
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Attack, null, this, messages[i].Payload));
                                    break;
                                }

                                if ((destinyPointer == null) && (destinyPoints != null) && (destinyPoints.Count > 0))
                                {
                                    destinyPointer = destinyPoints.GetEnumerator();
                                    destinyPointer.MoveNext();
                                    Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                    direction = vecTmp - Position;
                                }

                                if
                                (
                                    (!Double.IsNaN(direction.X)) &&
                                    (!Double.IsNaN(direction.Y)) &&
                                    (!Double.IsNaN(direction.Z))
                                )
                                {
                                    direction.Normalize();

                                    Rotation.Y = ComputeRadian(new Vector2(Position.X, Position.Z), new Vector2(direction.X, direction.Z));

                                    Position.X += direction.X * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                    Position.Z += direction.Z * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50.0f;
                                }

                                if
                                (
                                    (destinyPointer != null) &&
                                    (Math.Abs(Position.X - destinyPointer.Current.X) < 0.5f) &&
                                    (Math.Abs(Position.Z - destinyPointer.Current.Y) < 0.5f)
                                )
                                {
                                    // Next step walk.
                                    if (!destinyPointer.MoveNext())
                                    {
                                        EndMove(messages[i]);

                                        setIdle();
                                        Console.WriteLine("done");
                                    }
                                    else
                                    {
                                        Vector3 vecTmp = new Vector3(destinyPointer.Current.X, 0.0f, destinyPointer.Current.Y);
                                        direction = vecTmp - Position;
                                    }
                                }
                            }
                            else
                            {
                                EndMove(messages[i]);
                            }
                            break;
                        #endregion Move To Attack

                        #region Attack
                        case (int)EventManager.Events.Attack:

                            if (messages[i].Payload is Unit)
                            {
                                destinyUnit = (Unit)messages[i].Payload;
                                destinyBuilding = null;
                            }
                            else if (messages[i].Payload is Building)
                            {
                                destinyUnit = null;
                                destinyBuilding = (Building)messages[i].Payload;
                            }

                            if
                            (
                                    (
                                        (destinyUnit != null)
                                        &&
                                        (Math.Abs(Position.X - destinyUnit.Position.X) < range)
                                    )
                                    ||
                                    (
                                        (destinyBuilding != null)
                                        &&
                                        (Math.Abs(Position.Z - destinyBuilding.Position.Z) < range)
                                    )
                                )
                            {

                                if (EventManager.MessageToOld(gameTime, messages[i], ratio))
                                {
                                    if ((GameObject)messages[i].Payload is Unit)
                                    {
                                        ((Unit)messages[i].Payload).HP -= damage;

                                        if (((Unit)messages[i].Payload).dead)
                                        {
                                            messages[i].Done = true;
                                            setIdle();
                                            break;
                                        }
                                    }
                                    else if ((GameObject)messages[i].Payload is Building)
                                    {
                                        ((Building)messages[i].Payload).HP -= damage;

                                        if (((Building)messages[i].Payload).dead)
                                        {
                                            messages[i].Done = true;
                                            setIdle();
                                            break;
                                        }
                                    }

                                    messages[i].timer = gameTime.TotalGameTime;
                                    Game1.sounds[0].Play(1.0f, 0.0f, 0.0f);
                                    Console.WriteLine("Attack!");
                                }

                                if (messages[i].timer == TimeSpan.Zero)
                                {
                                    messages[i].timer = gameTime.TotalGameTime;
                                }
                            }
                            else
                            {
                                messages[i].Done = true;
                                setIdle();

                                if (messages[i].Payload is Unit)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, this, destinyUnit));
                                }
                                else if (messages[i].Payload is Building)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, this, destinyBuilding));
                                }
                            }

                            break;
                        #endregion Attack
                    }
            }
        }

        private void EndMove(Message _msg)
        {
            destinyPoints = null;
            destinyPointer = null;

            direction.X = 0.0f;
            direction.Z = 0.0f;

            _msg.Done = true;
        }

        private float ComputeRadian(Vector2 _beg, Vector2 _end)
        {
            Vector2 vecTmp = Vector2.Multiply(_beg, _end);

            vecTmp.Normalize();

            float dotProd = Vector2.Dot(vecTmp, new Vector2(0, 1));

            return (float)(Math.Acos(dotProd) * (180.0f / Math.PI));
        }

        private Unit FindEnemy()
        {
            foreach (Unit _uni in ((Game1)player.game).player.UnitList)
            {
                if (_uni.destinyUnit == this)
                {
                    return _uni;
                }
            }

            foreach (Unit _uni in ((Game1)player.game).enemy.UnitList)
            {
                if (_uni.destinyUnit == this)
                {
                    return _uni;
                }
            }

            return null;
        }

        public void setWalk()
        {
            this.currentModel.player.PlayClip("Walk", true);
        }
        public void setIdle()
        {
            this.currentModel.player.PlayClip("Idle", true);
        }
        public void setAttack()
        {
            this.currentModel.player.PlayClip("Attack", true);
        }
    }
}
