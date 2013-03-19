using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;


namespace Laikos
{
    class GameObject : Object
    {
        //miejsce na rozne pierdoly hp , mana sratatata (a generowane beda na podstawie pliku xml?)

        public GameObject(Model currentModelInput)
            :base(currentModelInput)
        {
            //tu ustawiamy rozne cuda
            Position = new Vector3(0, 7, 33);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            Rotation = new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(180), MathHelper.ToRadians(0));
            PlayAnimation("Take 001");//Play the default swimming animation
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

            base.Update(gameTime);
        }

        public override void Draw(Camera camera)
        {
            //a to zostawiamy na razie puste
            base.Draw(camera);
        }

    }
}
