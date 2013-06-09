//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Drawing;
//using System.IO;

//namespace PathFiding
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            DateTime[] czasy = new DateTime[3];

//            czasy[0] = DateTime.Now;
//            Map map = new Map();
//            map.loadMap(new Bitmap("C:\\Users\\Zielu\\Documents\\Visual Studio 2010\\Projects\\PathFiding\\PathFiding\\mapy\\mapkawysokosci.bmp"));

//            StreamWriter mapaKrokow = new StreamWriter("C:\\Users\\Zielu\\Documents\\Visual Studio 2010\\Projects\\PathFiding\\PathFiding\\mapa.txt", false);

//            for (int i = 0; i < map.Width; ++i)
//            {
//                for (int j = 0; j < map.Heigth; ++j)
//                {
//                    mapaKrokow.Write(map.WalkMeshMap[i, j].ToString());
//                }
//                mapaKrokow.WriteLine();
//            }

//            mapaKrokow.Close();
//            czasy[1] = DateTime.Now;
//            Wspolrzedne poczatek = new Wspolrzedne(0, 49);
//            Wspolrzedne koniec = new Wspolrzedne(49, 0);

//            ZnajdzSciezke.mapaUstaw(map.WalkMeshMap, map.Width, map.Heigth);

//            List<Wspolrzedne> sciezka = ZnajdzSciezke.obliczSciezke(poczatek, koniec);

//            for (int i = 0; i < sciezka.Count; ++i)
//            {
//                Console.Out.WriteLine(sciezka[i].X + " " + sciezka[i].Y);
//            }

//            czasy[2] = DateTime.Now;

//            Console.Out.WriteLine("Stworzenie mapy: " + (czasy[1] - czasy[0]).TotalMilliseconds);
//            Console.Out.WriteLine("Znalezienie ścieżki: " + (czasy[2] - czasy[1]).TotalMilliseconds);

//            Console.In.ReadLine();

//        }
//    }
//}
