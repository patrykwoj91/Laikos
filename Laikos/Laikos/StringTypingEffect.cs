using System;  
using System.Collections.Generic;  
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Audio;  
using Microsoft.Xna.Framework.Content;  
using Microsoft.Xna.Framework.GamerServices;  
using Microsoft.Xna.Framework.Graphics;  
using Microsoft.Xna.Framework.Input;  
using Microsoft.Xna.Framework.Media;  
using Microsoft.Xna.Framework.Net;  
using Microsoft.Xna.Framework.Storage;  

namespace Laikos
{
public class StringTypingEffect  
    {
       public enum Alignment { Center = 0, Left = 1, Right = 2, Top = 4, Bottom = 8 }
       public int i;  
        /// <summary>  
        /// Put your desire string here  
        /// In case of empty shows  
        /// "please enter a string at :*.WriteLine"  
        /// </summary>  
       public string WriteLine;  
       /// <summary>  
       /// Location of the text to start typing effect  
       /// In case of null it set to any random  
       /// </summary>  
       public Vector2 location;
       public string c;  
       bool typtime,blink;  
       SpriteBatch spriteBatch;  
       public StringTypingEffect(Game game,SpriteBatch spriteBatch)  
 
        {
            i = 0;
            c = "";
            this.spriteBatch = spriteBatch;
            if (location==null)  
            {  
                location.X = game.GraphicsDevice.Viewport.X / 2;  
                location.Y = game.GraphicsDevice.Viewport.Y/ 2;  
            }  
 
            if (WriteLine == null)  
            {  
                WriteLine = "<please enter a string at :*.WriteLine>";  
            }  
        }  
        /// <summary>  
        /// update the typing event  
        /// and handles the time limit with scoring.  
        /// </summary>  
//call it on xna main update(..)
        public void Update(GameTime gameTime)  
        {  
 
            typegenarator();  
            if (((gameTime.TotalGameTime.Milliseconds % 2) == 1))
                typtime = true;  
            else 
                typtime = false;  
 
            if (((gameTime.TotalGameTime.Milliseconds / 400) % 2) == 1)  
                blink = true;  
            else 
                blink = false;  
 
        }  
        /// <summary>  
        /// update the typing event
        /// </summary>  
        void typegenarator()  
        {  
 
 
 
 
            if (i < WriteLine.Length)  
            {  
                if (typtime)  
                {  
                    c = WriteLine.Substring(0, i) + "|";  
                    // System.Threading.Thread.Sleep(100);  
                    i++;  
                }  
                else 
                    c = WriteLine.Substring(0, i);//just for giving effect  
            }  
            else//this is for cursor blinking effect  
                if (blink)  
                {  
                    c = WriteLine;  
 
 
                }  
                else 
                {  
                    c = WriteLine + "|";  
 
                }  
 
 
        }


        public void DrawString(SpriteFont spfont, string text, Rectangle bounds, Alignment align, Color color)
        {
            Vector2 size = spfont.MeasureString(text);
            Vector2 pos = new Vector2(bounds.Center.X, bounds.Center.Y);
            Vector2 origin = size * 0.5f;

            if (align.HasFlag(Alignment.Left))
                origin.X += bounds.Width / 2 - size.X / 2;

            if (align.HasFlag(Alignment.Right))
                origin.X -= bounds.Width / 2 - size.X / 2;

            if (align.HasFlag(Alignment.Top))
                origin.Y += bounds.Height / 2 - size.Y / 2;

            if (align.HasFlag(Alignment.Bottom))
                origin.Y -= bounds.Height / 2 - size.Y / 2;

            spriteBatch.DrawString(spfont, text, pos, color, 0, origin, 1, SpriteEffects.None, 0);
        }
        /// <summary>  
        /// draw the typing effect;  
        /// place outside of spiritebatch  
        /// </summary>  
       public  void Draw(SpriteFont spfont)  
        {
           spriteBatch.DrawString(spfont,c, new Vector2(location.X,location.Y), Color.Green);   
        }  
 
    }  
 
} 