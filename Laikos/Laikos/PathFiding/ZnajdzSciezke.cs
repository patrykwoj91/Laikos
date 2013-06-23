using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos.PathFiding
{
    public class ZnajdzSciezke
    {
        private const int SKALA = 5;

        private int[,] mapa;

        public void mapaUstaw()
        {
            mapa = Map.WalkMeshMap;
            wymiarX = Map.Width;
            wymiarY = Map.Height;
        }

        public void mapaUstaw(int[,] _mapa, int _szerokosc, int _wysokosc)
        {
            mapa = _mapa;
            wymiarX = _szerokosc;
            wymiarY = _wysokosc;
        }

        private int wymiarX = 50;

        private int wymiarY = 50;

        private List<Wspolrzedne> sciezka = new List<Wspolrzedne>();
        private List<Wezel> sciezkaPrzeszukiwana;

        public List<Wspolrzedne> obliczSciezke(Wspolrzedne _poczatek, Wspolrzedne _koniec, int[,] _mapa)
        {
            int[,] mapaTmp = mapa;
            mapa = _mapa;

            List<Wspolrzedne> wynik = obliczSciezke(_poczatek, _koniec);

            mapa = mapaTmp;
            return wynik;
        }

        public List<Wspolrzedne> obliczSciezke(Wspolrzedne _poczatek, Wspolrzedne _koniec)
        {
            _poczatek.X = _poczatek.X / SKALA;
            _poczatek.Y = _poczatek.Y / SKALA;

            _koniec.X = _koniec.X / SKALA;
            _koniec.Y = _koniec.Y / SKALA;

            //Console.Out.WriteLine("Początek: " + _poczatek.X + ", " + _poczatek.Y + ", " + mapa[_poczatek.X, _poczatek.Y]);
            //Console.Out.WriteLine("Koniec: " + _koniec.X + ", " + _koniec.Y + ", " + mapa[_koniec.X, _koniec.Y]);

            if (mapa[_koniec.X, _koniec.Y] == 1)
            {
                return new List<Wspolrzedne>();
            }

            sciezkaPrzeszukiwana = new List<Wezel>();

            Wezel obecny = new Wezel();
            obecny.Wspolrzedne.X = _poczatek.X;
            obecny.Wspolrzedne.Y = _poczatek.Y;
            obecny.Waga = kosztSciezki(obecny) + funkcjaHeurystyczna(_poczatek, _koniec);

            sciezkaPrzeszukiwana.Add(obecny);

            bool liczDalej = true;//liczDalej = !obliczDzieci(obecny, _koniec);
            bool cofa = false;

            while (liczDalej)
            {
                if (obecny.Sprawdzony)
                {
                    obecny = obecny.Ojciec;

                    if (obecny == null)
                    {
                        Console.WriteLine("Nie znaleziono drogi.");

                        return new List<Wspolrzedne>();
                    }

                    cofa = true;
                }
                else
                {
                    if ((!cofa) && (obecny.Dzieci.Count == 0))
                    {
                        liczDalej = !obliczDzieci(obecny, _koniec);
                    }

                    cofa = false;

                    int iloscDzieciSprawdzonych = 0;

                    for (int dziecko = 0; dziecko < obecny.Dzieci.Count; ++dziecko)
                    {
                        if (obecny.Dzieci[dziecko].Sprawdzony)
                        {
                            ++iloscDzieciSprawdzonych;
                        }
                    }

                    int idNajlepszegoWezla = 0;

                    for (int najlepszy = 0; najlepszy < sciezkaPrzeszukiwana.Count; ++najlepszy)
                    {
                        if
                        (
                            (
                                (sciezkaPrzeszukiwana[idNajlepszegoWezla].Wspolrzedne.X != sciezkaPrzeszukiwana[najlepszy].Wspolrzedne.X)
                                ||
                                (sciezkaPrzeszukiwana[idNajlepszegoWezla].Wspolrzedne.Y != sciezkaPrzeszukiwana[najlepszy].Wspolrzedne.Y)
                            )
                            &&
                            (
                                !sciezkaPrzeszukiwana[najlepszy].Sprawdzony
                            )
                        )
                        {
                            idNajlepszegoWezla = najlepszy;
                            break;
                        }
                    }

                    for (int najlepszy = 0; najlepszy < sciezkaPrzeszukiwana.Count; ++najlepszy)
                    {
                        if (
                            (
                                (sciezkaPrzeszukiwana[idNajlepszegoWezla].Wspolrzedne.X != sciezkaPrzeszukiwana[najlepszy].Wspolrzedne.X)
                                ||
                                (sciezkaPrzeszukiwana[idNajlepszegoWezla].Wspolrzedne.Y != sciezkaPrzeszukiwana[najlepszy].Wspolrzedne.Y)
                            )
                            &&
                            (
                                !sciezkaPrzeszukiwana[najlepszy].Sprawdzony
                            )
                            &&
                            (
                                sciezkaPrzeszukiwana[idNajlepszegoWezla].Waga > sciezkaPrzeszukiwana[najlepszy].Waga
                            )
                           )
                        {
                            idNajlepszegoWezla = najlepszy;
                        }
                    }

                    obecny = sciezkaPrzeszukiwana[idNajlepszegoWezla];
                }
            };

            sciezka = new List<Wspolrzedne>();

            for (int wezel = 0; wezel < sciezkaPrzeszukiwana.Count; ++wezel)
            {
                if (sciezkaPrzeszukiwana[wezel].NalezyDoRozwiazania)
                {
                    sciezka.Add(new Wspolrzedne(sciezkaPrzeszukiwana[wezel].Wspolrzedne.X * SKALA, sciezkaPrzeszukiwana[wezel].Wspolrzedne.Y * SKALA));
                }
            }

            return sciezka;
        }

        private KOLEJNOSC odwrocRuch(KOLEJNOSC _ruch)
        {
            switch (_ruch)
            {
                case KOLEJNOSC.GORA:
                    return KOLEJNOSC.DOL;
                case KOLEJNOSC.GORA_PRAWO:
                    return KOLEJNOSC.DOL_LEWO;
                case KOLEJNOSC.PRAWO:
                    return KOLEJNOSC.LEWO;
                case KOLEJNOSC.DOL_PRAWO:
                    return KOLEJNOSC.GORA_LEWO;
                case KOLEJNOSC.DOL:
                    return KOLEJNOSC.GORA;
                case KOLEJNOSC.DOL_LEWO:
                    return KOLEJNOSC.GORA_PRAWO;
                case KOLEJNOSC.LEWO:
                    return KOLEJNOSC.PRAWO;
                case KOLEJNOSC.GORA_LEWO:
                    return KOLEJNOSC.DOL_PRAWO;
            }

            return KOLEJNOSC.GORA;
        }

        private KOLEJNOSC obliczRuch(Wspolrzedne _poczatek, Wspolrzedne _koniec)
        {
            if ((_koniec.Y < _poczatek.Y) && (_koniec.X == _poczatek.X))
            {
                return KOLEJNOSC.GORA;
            }
            else if ((_koniec.Y < _poczatek.Y) && (_koniec.X > _poczatek.X))
            {
                return KOLEJNOSC.GORA_PRAWO;
            }
            else if ((_koniec.X > _poczatek.X) && (_koniec.Y == _poczatek.Y))
            {
                return KOLEJNOSC.PRAWO;
            }
            else if ((_koniec.X > _poczatek.X) && (_koniec.Y > _poczatek.Y))
            {
                return KOLEJNOSC.DOL_PRAWO;
            }
            else if ((_koniec.Y > _poczatek.Y) && (_koniec.X == _poczatek.X))
            {
                return KOLEJNOSC.DOL;
            }
            else if ((_koniec.Y > _poczatek.Y) && (_koniec.X < _poczatek.X))
            {
                return KOLEJNOSC.DOL_LEWO;
            }
            else if ((_koniec.X < _poczatek.X) && (_koniec.Y == _poczatek.Y))
            {
                return KOLEJNOSC.LEWO;
            }
            else
            {
                return KOLEJNOSC.GORA_LEWO;
            }
        }

        private bool obliczDzieci(Wezel _obecny, Wspolrzedne _warunekStopu)
        {
            _obecny.Sprawdzony = true;

            for (int _kolejny = 0; _kolejny < 8; ++_kolejny)
            {
                //Pominięcie drogi, którą się przyszło.
                if ((_obecny.Ojciec != null) && ((KOLEJNOSC)_kolejny == odwrocRuch(_obecny.OstatniRuch)))
                {
                    continue;
                }

                // Jeśli ruch w daną stronę natrafia na ścianę.
                if (czySciana(_obecny.Wspolrzedne, (KOLEJNOSC)_kolejny))
                {
                    continue;
                }

                Wezel nowy = new Wezel();

                switch ((KOLEJNOSC)_kolejny)
                {
                    case KOLEJNOSC.GORA:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y - 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X;
                        break;
                    case KOLEJNOSC.GORA_PRAWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y - 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X + 1;
                        break;
                    case KOLEJNOSC.PRAWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X + 1;
                        break;
                    case KOLEJNOSC.DOL_PRAWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y + 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X + 1;
                        break;
                    case KOLEJNOSC.DOL:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y + 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X;
                        break;
                    case KOLEJNOSC.DOL_LEWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y + 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X - 1;
                        break;
                    case KOLEJNOSC.LEWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X - 1;
                        break;
                    case KOLEJNOSC.GORA_LEWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y - 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X - 1;
                        break;
                }

                // Jeśli nie powiodło się obliczenie współrzędnych nowego węzła.
                if ((nowy.Wspolrzedne.X == -1) || (nowy.Wspolrzedne.Y == -1))
                {
                    continue;
                }

                nowy.Waga = kosztSciezki(_obecny) + funkcjaHeurystyczna(_obecny.Wspolrzedne, _warunekStopu);
                nowy.OstatniRuch = obliczRuch(_obecny.Wspolrzedne, nowy.Wspolrzedne);

                if (wezelJuzIstnieje(nowy))
                {
                    //Połączenie węzłów ojca i dziecka.
                    _obecny.Dzieci.Add(nowy);
                    nowy.Ojciec = _obecny;

                    sciezkaPrzeszukiwana.Add(nowy);
                }

                // Sprawdzenie, czy nie odnaleziono rozwiązania.
                if ((nowy.Wspolrzedne.X == _warunekStopu.X) && (nowy.Wspolrzedne.Y == _warunekStopu.Y))
                {
                    Wezel licznikSciezki = nowy;

                    while (licznikSciezki.Ojciec != null)
                    {
                        licznikSciezki.NalezyDoRozwiazania = true;
                        licznikSciezki = licznikSciezki.Ojciec;
                    }

                    return true;
                }
            }

            return false;
        }

        private bool wezelJuzIstnieje(Wezel _nowy)
        {
            foreach (Wezel _wezel in sciezkaPrzeszukiwana)
            {
                if ((_nowy.Wspolrzedne.X == _wezel.Wspolrzedne.X) && (_nowy.Wspolrzedne.Y == _wezel.Wspolrzedne.Y))
                {
                    return false;
                }
            }

            return true;
        }

        private bool czySciana(Wspolrzedne _obecny, KOLEJNOSC _ruch)
        {
            switch (_ruch)
            {
                case KOLEJNOSC.GORA:
                    return !((_obecny.Y - 1 >= 0) && (mapa[_obecny.X, _obecny.Y - 1] == 0));
                case KOLEJNOSC.GORA_PRAWO:
                    return !((_obecny.Y - 1 >= 0) && (_obecny.X + 1 < wymiarX / SKALA) && (mapa[_obecny.X + 1, _obecny.Y - 1] == 0));
                case KOLEJNOSC.PRAWO:
                    return !((_obecny.X + 1 < wymiarX / SKALA) && (mapa[_obecny.X + 1, _obecny.Y] == 0));
                case KOLEJNOSC.DOL_PRAWO:
                    return !((_obecny.Y + 1 < wymiarY / SKALA) && (_obecny.X + 1 < wymiarX / SKALA) && (mapa[_obecny.X + 1, _obecny.Y + 1] == 0));
                case KOLEJNOSC.DOL:
                    return !((_obecny.Y + 1 < wymiarY / SKALA) && (mapa[_obecny.X, _obecny.Y + 1] == 0));
                case KOLEJNOSC.DOL_LEWO:
                    return !((_obecny.Y + 1 < wymiarY / SKALA) && (_obecny.X - 1 >= 0) && (mapa[_obecny.X - 1, _obecny.Y + 1] == 0));
                case KOLEJNOSC.LEWO:
                    return !((_obecny.X - 1 >= 0) && (mapa[_obecny.X + 1, _obecny.Y] == 0));
                case KOLEJNOSC.GORA_LEWO:
                    return !((_obecny.Y - 1 >= 0) && (_obecny.X - 1 >= 0) && (mapa[_obecny.X - 1, _obecny.Y + 1] == 0));
            }

            return false;
        }

        private double kosztSciezki(Wezel _obecny)
        {
            Wezel licznikSciezki = _obecny;
            int dlugoscSciezki = 0;

            while (licznikSciezki.Ojciec != null)
            {
                ++dlugoscSciezki;
                licznikSciezki = licznikSciezki.Ojciec;
            }

            return dlugoscSciezki;
        }

        private double funkcjaHeurystyczna(Wspolrzedne _obecny, Wspolrzedne _koniec)
        {
            return Math.Abs(_obecny.X - _koniec.X) + Math.Abs(_obecny.Y - _koniec.Y);
        }

    }
}
