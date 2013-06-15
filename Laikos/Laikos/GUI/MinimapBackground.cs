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
    class MinimapBackground
    {
        private static Texture2D minimapBackground;
        private static Rectangle position;

        public static void Initialize(ContentManager content)
        {
            minimapBackground = content.Load<Texture2D>("GUI/tlo_minimapa");
            position = new Rectangle(Minimap.diff, Minimap.diff, Minimap.width, Minimap.height);
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            position = new Rectangle(0, 0, Minimap.width + 2 * Minimap.diff, Minimap.height + 2 * Minimap.diff);

            spriteBatch.Draw(minimapBackground, position, Color.White);

            position = new Rectangle(Minimap.diff, Minimap.diff, Minimap.width, Minimap.height);

            if (SelectingGUI.MiniMapClicked(Input.currentMouseState.X, Input.currentMouseState.Y))
                spriteBatch.Draw((Texture2D)Minimap.miniMap, position, Color.White);
            else
                spriteBatch.Draw((Texture2D)Minimap.miniMap, position, Color.White * 0.7f);
        }
    }
}
