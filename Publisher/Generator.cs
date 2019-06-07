using System;
using System.Collections.Generic;
using System.Text;
using Utils;
using Utils.Models;

namespace Publisher
{
    public class Generator
    {
        private string identifier;
        public Generator(string identifier)
        {
            this.identifier = identifier;
        }

        public List<Publication> Generate()
        {
            var publications = new List<Publication>();
            string[] lines = FileReader.ReadAllLines(Constants.PublicationsFileName);

            foreach (string line in lines)
            {
                var publication = new Publication
                {
                    Contents = line.Replace("\0", ""),
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    PublisherId = $"P{identifier}"
                };
                publications.Add(publication);
            }

            return publications;
        }
    }
}
