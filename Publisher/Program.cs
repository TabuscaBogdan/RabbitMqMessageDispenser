using System;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;
using Utils;
using Utils.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static List<Publication> publications = new List<Publication>();

        static void Main(string[] args)
        {

            var publisherId = "";

            if (args.Length == 0)
            {
                //Console.WriteLine("Enter publisher id:");
                publisherId = Console.ReadLine();
            }
            else
            {
                publisherId = args[0];
                Console.WriteLine($"Publisher id: {publisherId}");
            }


            try
            {
                var generator = new Generator(publisherId);
                publications = generator.Generate();
                var factory = RabbitFactory.GetFactory();
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: exchangeAgent, durable: true, exclusive: false, autoDelete: false, arguments: null);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        //Stopwatch sw = new Stopwatch();
                        //sw.Start();
                        //while (sw.Elapsed.TotalMinutes < 5)
                        //{
                            foreach (var publication in publications)
                            {
                                SendToQueue(channel, properties, publication);
                            }
                        //}
                        //sw.Stop();
                        Console.WriteLine($"Publisher P{publisherId} sent all publications.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            Console.ReadLine();
        }
        private static void SendToQueue(IModel channel, IBasicProperties properties, Publication publication)
        {
            var bytes = ProtoSerialization.SerializeAndGetBytes(publication);
            channel.BasicPublish(exchange: "", routingKey: exchangeAgent, basicProperties: properties, body: bytes);
            //Console.WriteLine($" [*] Sent publication: {publication} |");
        }
    }
}
