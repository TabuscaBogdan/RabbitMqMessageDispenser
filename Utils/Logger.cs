using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public static class Logger
    {
        public static void Log(string log, bool force = false)
        {
            if (Constants.Debug || force)
            {
                Console.WriteLine(log);
            }
        }
    }
}
