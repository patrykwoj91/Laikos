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
        private const int SKALA = 10;

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
        public static int Height
        {
            get { return height; }
        }

        public static void loadMap(Texture2D _map)
        {
            width = _map.Width;
            height = _map.Height;

            int[] mapTexTmp = new int[width * height];
            _map.GetData<int>(mapTexTmp);

            int[,] mapTmp = new int[Width / SKALA + 1, Height / SKALA + 1];

            int iloscDobrych = 0;
            int iloscZlych = 0;
            int licznik = 0;

            for (int widthTmp = 0; widthTmp < Width / SKALA; ++widthTmp)
            {
                for (int heightTmp = 0; heightTmp < Height / SKALA; ++heightTmp)
                {
                    for (int i = 0; i < SKALA; ++i)
                    {
                        for (int j = 0; j < SKALA; ++j)
                        {
                            Color bitmapColor = Color.FromArgb(mapTexTmp[(widthTmp * SKALA * width + heightTmp * SKALA) + (i * width) + j]);

                            if ((bitmapColor.R >= 15) && (bitmapColor.R < 60))
                            {
                                //mapTmp[heightTmp, widthTmp] = 0;
                                ++iloscDobrych;
                            }
                            else
                            {
                                ++iloscZlych;
                                //mapTmp[heightTmp, widthTmp] = 1;
                            }
                        }
                    }

                    if ((iloscDobrych / SKALA) > iloscZlych)
                    {
                        mapTmp[licznik % (height / SKALA), licznik / (width / SKALA)] = 0;
                    }
                    else
                    {
                        mapTmp[licznik % (height / SKALA), licznik / (width / SKALA)] = 1;
                    }

                    iloscDobrych = 0;
                    iloscZlych = 0;

                    ++licznik;
                }
                Console.Out.Write(".");
            }

            Console.Out.WriteLine();

            WalkMeshMap = mapTmp;
        }
    }
}
