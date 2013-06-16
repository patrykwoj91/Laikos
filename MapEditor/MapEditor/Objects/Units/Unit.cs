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
        public double HP;
        public double maxHP;

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
        }

        public void Update(GameTime gameTime)
        {
            HandleEvent(gameTime);

            base.Update(gameTime);
        }

        public override void HandleEvent(GameTime gameTime)
        {

        }

    }
}
