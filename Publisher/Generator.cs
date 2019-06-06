using System;
using System.Collections.Generic;
using System.Text;

namespace Publisher
{
    public class Generator
    {
        private int numberOfSubscriptions = 0;
        private string identifier;
        public Generator(int numberOfSubscriptions, string identifier)
        {
            this.identifier = identifier;
            this.numberOfSubscriptions = numberOfSubscriptions;
        }

        //the generated publications are read from file
        public List<string> Generate(string publisherIdentifier)
        {
            var fileName = System.IO.Path.Combine(Environment.CurrentDirectory, "publications.txt");
            return readFromFile(publisherIdentifier,fileName);
        }

        public List<string> readFromFile(string publisherIdentifier,string fileName){
        var subs = new List<string>();
        string[] lines = System.IO.File.ReadAllLines(fileName);

        foreach (string line in lines)
        {
            subs.Add($"P{publisherIdentifier}:"+line.Replace("\0", ""));
        }

            return subs;
        }
    }
}
