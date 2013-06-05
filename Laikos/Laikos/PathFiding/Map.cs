using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Laikos
{
    class Map
    {
        private int[,] map;
        public int[,] WalkMeshMap
        {
            get { return map; }
            set { map = value; }
        }

        private int width;
        public int Width
        {
            get { return width; }
        }
        private int height;
        public int Heigth
        {
            get { return height; }
        }
    
        public void loadMap (Bitmap _map)
        {
            width = _map.Width;
            height = _map.Height;

            int[,] mapTmp = new int[Width, Heigth];

            for (int widthTmp = 0; widthTmp < _map.Width; ++widthTmp)
            {
                for (int heightTmp = 0; heightTmp < _map.Height; ++heightTmp)
                {
                    if (_map.GetPixel(widthTmp, heightTmp).R < 50)
                    {
                        mapTmp[widthTmp, heightTmp] = 0;
                    }
                    else
                    {
                        mapTmp[widthTmp, heightTmp] = 1;
                    }
                }
                Console.Out.Write(".");
            }

            Console.Out.WriteLine();

            WalkMeshMap = mapTmp;
        }
    }
}
