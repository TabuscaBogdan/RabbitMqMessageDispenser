using System;
using System.Collections.Generic;
using System.Text;

namespace Publisher
{
    public class Generator
    {
        private int numberOfSubscriptions = 0;
        private string identifier;
        private Random rng;
        public Generator(int numberOfSubscriptions, string identifier)
        {
            this.identifier = identifier;
            this.numberOfSubscriptions = numberOfSubscriptions;
            rng=new Random();
        }

        //TODO: add a propper subscription generation function
        public string Generate()
        {
            if (numberOfSubscriptions > 0)
            {
                numberOfSubscriptions--;
                return $"{identifier}:{rng.Next()}";
            }
            return "";
        }
    }
}
