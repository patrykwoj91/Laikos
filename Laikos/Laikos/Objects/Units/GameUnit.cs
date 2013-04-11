using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
    class GameUnit : Unit
    {
        //miejsce na rozne pierdoly hp , mana sratatata (a generowane beda na podstawie pliku xml?)
        bool walk, idle;

        public GameUnit(Model currentModelInput)
            :base(currentModelInput)
        {
            //tu ustawiamy rozne cuda
            Position = new Vector3(0, 50, 33);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            Scale = 0.05f;
            Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(180), MathHelper.ToRadians(0));
            walk = false;
            idle = true;
            PlayAnimation("Idle");//Play the default animation temporary
            
        }

        public override void Update(GameTime gameTime)
        {
            if (walk)
            {
                PlayAnimation("Walk");
            }
            else
            {
                PlayAnimation("Idle");
            }

            Collisions.AddGravity(ref Position);
            HandleInput();
            Collisions.CheckWithTerrain(ref Position, 0.5f);

            base.Update(gameTime);
        }

        public override void Draw()
        {
            
            base.Draw();
        }

        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                if (!walk) { walk = !walk;}
                Position.Z += 0.1f;
                Rotation.Y = MathHelper.ToRadians(180);
            }

            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                if (!walk) { walk = !walk;}
                Position.Z -= 0.1f;
                Rotation.Y = MathHelper.ToRadians(0);
                
            }

            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                if (!walk) { walk = !walk;}
                Position.X -= 0.1f;
                Rotation.Y = MathHelper.ToRadians(90);
                
            }
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                if (!walk) { walk = !walk;}
                Position.X += 0.1f;
                Rotation.Y = MathHelper.ToRadians(-90);
            }

            if (currentKeyboardState.IsKeyUp(Keys.D) && currentKeyboardState.IsKeyUp(Keys.S) && currentKeyboardState.IsKeyUp(Keys.A) && currentKeyboardState.IsKeyUp(Keys.W))
            {
                walk = false;
            }
        }
    }
}
