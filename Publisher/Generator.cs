using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace Publisher
{
    public class Generator
    {
        private string identifier;
        public Generator(string identifier)
        {
            this.identifier = identifier;
        }

        //the generated publications are read from file
        public List<string> Generate()
        {
           // return readFromFile(publisherIdentifier, Constants.PublicationsFileName);
            var subs = new List<string>();
            string[] lines = FileReader.ReadAllLines(Constants.PublicationsFileName);

            foreach (string line in lines)
            {
                subs.Add($"P{identifier}:" + line.Replace("\0", ""));
            }

            return subs;
        }
    }
}
