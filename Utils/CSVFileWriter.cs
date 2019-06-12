using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils
{
    public class CSVFileWriter
    {
        private int identifier = -1;
        private Dictionary<String, List<Dictionary<String, decimal>>> latencyDict = new Dictionary<String, List<Dictionary<String, decimal>>>();
        private String file = "";

        public CSVFileWriter(int identifier, Dictionary<String, List<Dictionary<String, decimal>>> dict)
        {
            this.identifier = identifier;
            this.latencyDict = dict;
            file = String.Format(Constants.LatencyOutputFileName, identifier);
        }

        public void WriteAllLatenciesInCSV()
        {
            //String key = String.Format("C{0}", "1");
            //if (!latencyDict.ContainsKey(key))
            //{
            //    latencyDict.Add(key, new List<Dictionary<String, decimal>>());
            //}
            //latencyDict[key].Add(new Dictionary<String, decimal>()
            //            {
            //                {"VLAD", 88},
            //                {"CATI", 96},
            //                {"Simona", 96}
            //            });
            // key = String.Format("C{0}", "3");
            //if (!latencyDict.ContainsKey(key))
            //{
            //    latencyDict.Add(key, new List<Dictionary<String, decimal>>());
            //}
            //latencyDict[key].Add(new Dictionary<String, decimal>()
                        //{
                        //    {"VLAD", 21},
                        //    {"CATI", 10},
                        //    {"Simona", 31}
                        //});
            using (var stream = File.CreateText(file))
            {
                Console.WriteLine($"Started to write in the file {file}.");
                foreach (KeyValuePair<String, List<Dictionary<String, decimal>>> item in latencyDict)
                {
                    String temp = item.Key;
                    if (temp.Contains(String.Format("C{0}", identifier)))
                    {
                        Console.WriteLine($"List: {item.Value}");
                        for (int i = 0;  i < item.Value.Count; i++)
                        {
                            Console.WriteLine($"In list: {item.Value[i]}");
                            string csvRow = string.Format("{0},{1},{2}", item.Key, item.Value[i].ElementAt(0).Key, item.Value[i].ElementAt(0).Value);
                            stream.WriteLine(csvRow);
                        }
                    }
                }
                Console.WriteLine("Ended to write in the file.");
            }
        }
    }
}
