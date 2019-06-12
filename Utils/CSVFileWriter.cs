using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils
{
    public class CSVFileWriter
    {
        private String file = "";
        private static List<decimal> latencies = new List<decimal>();

        public CSVFileWriter(string identifier, List<decimal> latency)
        {
            latencies = latency;
            file = String.Format(Constants.LatencyOutputFileName, identifier);
        }

        public void WriteAllLatenciesInCSV()
        {
            using (var stream = File.CreateText(file))
            {
                decimal sum = 0;
                Console.WriteLine($"Started to write in the file {file}.");
                foreach (var item in latencies)
                {
                    string csvRow = string.Format("{0:0.00##}", item);
                    sum = sum + item;
                    stream.WriteLine(csvRow);
                }
                stream.WriteLine(string.Format("{0:0.00##}", sum / latencies.Count));

                Console.WriteLine("Ended to write in the file.");
            }
        }
    }
}
