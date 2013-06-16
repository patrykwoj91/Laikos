using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Laikos.PathFiding
{
    public static class Map
    {
        private const int SKALA = 5;

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

        public static void loadMap(Texture2D _map, DecorationManager _decorations)
        {
            width = _map.Width;
            height = _map.Height;

            //Terrain
            int[] mapTexTmp = new int[width * height];
            _map.GetData<int>(mapTexTmp);
            
            int[,] mapTmp = new int[Width / SKALA + 1, Height / SKALA + 1];

            //Initialize map.
            for (int widthTmp = 0; widthTmp < Width / SKALA; ++widthTmp)
            {
                for (int heightTmp = 0; heightTmp < Height / SKALA; ++heightTmp)
                {
                    mapTmp[widthTmp, heightTmp] = 0;
                }
            }

            //Decorations
            foreach (Decoration _dec in _decorations.DecorationList)
            {
                Vector2 size = _dec.Size / SKALA;
                Vector3 position = _dec.Position / SKALA;

                position.X -= (int)size.X / 2 - 1;
                position.Z -= (int)size.Y / 2 - 1;

                for (int i = 0; i < size.X; ++i)
                {
                    for (int j = 0; j < size.Y; ++j)
                    {
                        mapTmp[(int)position.X + i, (int)position.Z + j] = 1;
                    }
                }
            }

            //Creating walkmesh
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
                            System.Drawing.Color bitmapColor = System.Drawing.Color.FromArgb(mapTexTmp[(widthTmp * SKALA * width + heightTmp * SKALA) + (i * width) + j]);

                            if ((bitmapColor.R >= 15) && (bitmapColor.R < 150))
                            {
                                ++iloscDobrych;
                            }
                            else
                            {
                                ++iloscZlych;
                            }
                        }
                    }

                    if ((iloscDobrych / SKALA) <= iloscZlych)
                    {
                        mapTmp[licznik % (height / SKALA), licznik / (width / SKALA)] = 1;
                    }

                    iloscDobrych = 0;
                    iloscZlych = 0;

                    ++licznik;
                }
            }

            WalkMeshMap = mapTmp;
        }
    }
}
