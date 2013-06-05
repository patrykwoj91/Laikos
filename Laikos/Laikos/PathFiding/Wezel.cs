using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos
{
    class Wezel
    {
        private List<Wezel> dzieci;
        public List<Wezel> Dzieci
        {
            get { return dzieci; }
            set { dzieci = value; }
        }

        private Wezel ojciec;
        public Wezel Ojciec
        {
            get { return ojciec; }
            set { ojciec = value; }
        }

        private Wspolrzedne wspolrzedne;
        public Wspolrzedne Wspolrzedne
        {
            get { return wspolrzedne; }
            set { wspolrzedne = value; }
        }

        private bool nalezyDoRozwiazania;
        public bool NalezyDoRozwiazania
        {
            get { return nalezyDoRozwiazania; }
            set { nalezyDoRozwiazania = value; }
        }

        private double waga;
        public double Waga
        {
            get { return waga; }
            set { waga = value; }
        }

        private bool sprawdzony;
        public bool Sprawdzony
        {
            get { return sprawdzony; }
            set { sprawdzony = value; }
        }

        private KOLEJNOSC ostatniRuch = KOLEJNOSC.GORA;
        public KOLEJNOSC OstatniRuch
        {
            get { return ostatniRuch; }
            set { ostatniRuch = value; }
        }

        public Wezel()
        {
            dzieci = new List<Wezel>();
            wspolrzedne = new Wspolrzedne(-1, -1);
            nalezyDoRozwiazania = false;
            waga = 0;
            sprawdzony = false;
        }

    }
}
