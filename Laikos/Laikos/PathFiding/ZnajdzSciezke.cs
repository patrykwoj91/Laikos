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
        private int[,] mapa;

        public void mapaUstaw()
        {
            mapa = Map.WalkMeshMap;
            wymiarX = Map.Width;
            wymiarY = Map.Heigth;
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
            Console.Out.WriteLine("Początek: " + _poczatek.X + ", " + _poczatek.Y + ", " + mapa[_poczatek.X, _poczatek.Y]);
            Console.Out.WriteLine("Koniec: " + _koniec.X + ", " + _koniec.Y + ", " + mapa[_koniec.X, _koniec.Y]);

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
                        Console.Out.WriteLine("Nie znaleziono drogi.");

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
                                sciezkaPrzeszukiwana[idNajlepszegoWezla].Waga >= sciezkaPrzeszukiwana[najlepszy].Waga
                            )
                           )
                        {
                            idNajlepszegoWezla = najlepszy;
                        }
                    }

                    obecny = sciezkaPrzeszukiwana[idNajlepszegoWezla];

                    //Console.Out.WriteLine(obecny.Wspolrzedne.X + " " + obecny.Wspolrzedne.Y + " " + obecny.Waga);
                    //Console.In.ReadLine();
                }
            };

            for (int wezel = 0; wezel < sciezkaPrzeszukiwana.Count; ++wezel)
            {
                if (sciezkaPrzeszukiwana[wezel].NalezyDoRozwiazania)
                {
                    sciezka.Add(sciezkaPrzeszukiwana[wezel].Wspolrzedne);
                }
            }

            for (int i = 0; i < sciezkaPrzeszukiwana.Count; ++i)
            {
                //Console.Out.WriteLine(sciezkaPrzeszukiwana[i].Wspolrzedne.X + " " + sciezkaPrzeszukiwana[i].Wspolrzedne.Y + " " + sciezkaPrzeszukiwana[i].Waga);
            }

            return sciezka;
        }

        //private static void wyrysujPlansze(Wspolrzedne _obecny)
        //{
        //    for (int i = 0; i < 
        //}

        private KOLEJNOSC odwrocRuch(KOLEJNOSC _ruch)
        {
            switch (_ruch)
            {
                case KOLEJNOSC.GORA:
                    return KOLEJNOSC.DOL;
                case KOLEJNOSC.PRAWO:
                    return KOLEJNOSC.LEWO;
                case KOLEJNOSC.DOL:
                    return KOLEJNOSC.GORA;
                case KOLEJNOSC.LEWO:
                    return KOLEJNOSC.PRAWO;
            }

            return KOLEJNOSC.GORA;
        }

        private KOLEJNOSC obliczRuch(Wspolrzedne _poczatek, Wspolrzedne _koniec)
        {
            if (_koniec.Y < _poczatek.Y)
            {
                return KOLEJNOSC.GORA;
            }
            else if (_koniec.X > _poczatek.X)
            {
                return KOLEJNOSC.PRAWO;
            }
            else if (_koniec.Y > _poczatek.Y)
            {
                return KOLEJNOSC.DOL;
            }
            else if (_koniec.X < _poczatek.X)
            {
                return KOLEJNOSC.LEWO;
            }

            return KOLEJNOSC.GORA;
        }

        private bool obliczDzieci(Wezel _obecny, Wspolrzedne _warunekStopu)
        {
            _obecny.Sprawdzony = true;

            for (int _kolejny = 0; _kolejny < 4; ++_kolejny)
            {
                //Pominięcie drogi, którą się przyszło.
                if ((_obecny.Ojciec != null) && (KOLEJNOSC)_kolejny == odwrocRuch(_obecny.OstatniRuch))
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
                    case KOLEJNOSC.PRAWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X + 1;
                        break;
                    case KOLEJNOSC.DOL:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y + 1;
                        nowy.Wspolrzedne.X = _obecny.Wspolrzedne.X;
                        break;
                    case KOLEJNOSC.LEWO:
                        nowy.Wspolrzedne.Y = _obecny.Wspolrzedne.Y;
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
                case KOLEJNOSC.PRAWO:
                    return !((_obecny.X + 1 < wymiarX) && (mapa[_obecny.X + 1, _obecny.Y] == 0));
                case KOLEJNOSC.DOL:
                    return !((_obecny.Y + 1 < wymiarY) && (mapa[_obecny.X, _obecny.Y + 1] == 0));
                case KOLEJNOSC.LEWO:
                    return !((_obecny.X - 1 >= 0) && (mapa[_obecny.X - 1, _obecny.Y] == 0));
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
