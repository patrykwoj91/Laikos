using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos.PathFiding
{
    public class Wspolrzedne
    {
        private int x;
        public int X
        {
            get { return x; }
            set { x = value; }
        }

        private int y;
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Wspolrzedne(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }
    }
}
