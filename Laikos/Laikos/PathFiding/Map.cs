using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Microsoft.Xna.Framework.Graphics;

namespace Laikos.PathFiding
{
    public static class Map
    {
        private static int[,] map;
        public static int[,] WalkMeshMap
        {
            get { return map; }
            set { map = value; }
        }

        private static int width;
        public static int Width
        {
            get { return width; }
        }
        private static int height;
        public static int Heigth
        {
            get { return height; }
        }

        public static void loadMap(Texture2D _map)
        {
            width = _map.Width;
            height = _map.Height;

            int[] mapTexTmp = new int[width * height];
            _map.GetData<int>(mapTexTmp);

            int[,] mapTmp = new int[Width, Heigth];

            for (int widthTmp = 0; widthTmp < _map.Width; ++widthTmp)
            {
                for (int heightTmp = 0; heightTmp < _map.Height; ++heightTmp)
                {
                    Color bitmapColor = Color.FromArgb(mapTexTmp[widthTmp * width + heightTmp]);

                    if ((bitmapColor.R >= 0) && (bitmapColor.R < 20))
                    {
                        mapTmp[heightTmp, widthTmp] = 0;
                    }
                    else
                    {
                        mapTmp[heightTmp, widthTmp] = 1;
                    }
                }
                Console.Out.Write(".");
            }

            Console.Out.WriteLine();

            WalkMeshMap = mapTmp;
        }
    }
}
