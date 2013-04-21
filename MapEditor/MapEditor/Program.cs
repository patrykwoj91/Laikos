using System;

namespace MapEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MapEditor game = new MapEditor())
            {
                game.Run();
            }
        }
    }
#endif
}

