using System;

namespace Manhattanville
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MARVIN game = new MARVIN())
            {
                game.Run();
            }
        }
    }
}

