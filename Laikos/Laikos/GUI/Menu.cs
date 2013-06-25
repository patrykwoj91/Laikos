using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{
    class Menu
    {
        private SpriteBatch spriteBatch;
        private Texture2D tlo;
        private Texture2D exit;
        private Texture2D laikos;
        private Texture2D newGame;
        private Texture2D kwadrat;
        PresentationParameters pp;
        public static bool inMenu;
        Rectangle tloPosition;
        Rectangle laikosPosition;
        Rectangle newGamePosition;
        Rectangle exitPosition;
        Rectangle kwadratPosition1;
        Rectangle kwadratPosition2;

        public Menu(SpriteBatch spriteBatch, ContentManager content, PresentationParameters pp)
        {
            this.spriteBatch = spriteBatch;
            tlo = content.Load<Texture2D>("Menu/tlo");
            exit = content.Load<Texture2D>("Menu/exit");
            laikos = content.Load<Texture2D>("Menu/laikos");
            newGame = content.Load<Texture2D>("Menu/new game");
            kwadrat = content.Load<Texture2D>("Menu/po najechaniu");
            tloPosition = new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
            laikosPosition = new Rectangle(pp.BackBufferWidth / 3, pp.BackBufferHeight / 5, pp.BackBufferWidth / 3, pp.BackBufferHeight / 5);
            newGamePosition = new Rectangle(pp.BackBufferWidth / 3 + pp.BackBufferWidth / 15, pp.BackBufferHeight / 5 + pp.BackBufferHeight / 5, 300, 100);
            exitPosition = new Rectangle(pp.BackBufferWidth / 3 + pp.BackBufferWidth / 9, pp.BackBufferHeight / 4 + pp.BackBufferHeight / 4, 200, 100);
            kwadratPosition1 = new Rectangle(0, 0, 0, 0);
            this.pp = pp;
            inMenu = true;
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(tlo, tloPosition, Color.White);
            spriteBatch.Draw(laikos, laikosPosition, Color.White);
            spriteBatch.Draw(newGame, newGamePosition, Color.White);
            spriteBatch.Draw(exit, exitPosition, Color.White);
            spriteBatch.Draw(kwadrat, kwadratPosition1, Color.White);
            spriteBatch.Draw(kwadrat, kwadratPosition2, Color.White);
            spriteBatch.End();
        }

        public void Update()
        {
            if (GUI.insideRectangle(newGamePosition))
            {
                kwadratPosition1 = new Rectangle(newGamePosition.X - 50, newGamePosition.Y, 50, 50);
                kwadratPosition2 = new Rectangle(newGamePosition.X + newGamePosition.Width, newGamePosition.Y, 50, 50);
                if (Input.oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    inMenu = false;

                    if (Game1.playIntro < 3)
                    {
                        Game1.videoPlayer.Play(Game1.video);
                    }
                }
            }
            else if (GUI.insideRectangle(exitPosition))
            {
                kwadratPosition1 = new Rectangle(exitPosition.X - 50, exitPosition.Y + exitPosition.Height / 4, 50, 50);
                kwadratPosition2 = new Rectangle(exitPosition.X + exitPosition.Width, exitPosition.Y + exitPosition.Height / 4, 50, 50);
            }
            else
            {
                kwadratPosition1 = new Rectangle(0, 0, 0, 0);
                kwadratPosition2 = new Rectangle(0, 0, 0, 0);
            }
        }
    }
}
