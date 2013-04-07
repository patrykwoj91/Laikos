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
        public AnimationClip clip; //Contains the animation clip currently playing

        //miejsce na rozne pierdoly hp , mana sratatata (a generowane beda na podstawie pliku xml?)

        public GameUnit(Model currentModelInput)
            :base(currentModelInput)
        {
            //tu ustawiamy rozne cuda
            Position = new Vector3(0, 50, 33);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            Scale = 0.05f;
            Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(180), MathHelper.ToRadians(0));

            PlayAnimation("Idle");//Play the default animation temporary
            
        }

        public static void Fire(string EventName)
        {
            Console.WriteLine("Firing");
        }

        public override void Update(GameTime gameTime)
        {
            //tu grawitacje, poruszanie i inne pierdoly np:

            //KeyboardState currentKeyboardState = Keyboard.GetState();
            //AnimationData animationData = currentModel.Tag as AnimationData;
            //if (currentKeyboardState.IsKeyDown(Keys.D1))
            //{
            //    AnimationClip clip = animationData.AnimationClips["greet"];
            //    animationPlayer.StartClip(clip);
            //}
            //else if (currentKeyboardState.IsKeyDown(Keys.D2))
            //{
            //    AnimationClip clip = animationData.AnimationClips["stand"];
            //    animationPlayer.StartClip(clip);
            //}
            //else if (currentKeyboardState.IsKeyDown(Keys.D3))
            //{
            //    AnimationClip clip = animationData.AnimationClips["jump"];
            //    animationPlayer.StartClip(clip);
            //}
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
                Position.Z += 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                Position.Z -= 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                Position.X -= 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                Position.X += 0.1f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                if (animationPlayer.CurrentClip.Name != "Idle")
                    PlayAnimation("Idle"); ;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                if (animationPlayer.CurrentClip.Name != "Fire")
                    PlayAnimation("Fire"); ;
            }
        }
    }
}
