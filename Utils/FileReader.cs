﻿namespace Utils
{
    public static class FileReader
    {
        public static string[] ReadAllLines(string path)
        {
            return System.IO.File.ReadAllLines(path);
        }
    }
}
