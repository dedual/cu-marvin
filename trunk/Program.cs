using System;

namespace MARVIN
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
                game.finalize();
            }
        }
    }
}

