using System;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;
using Utils;
using Utils.Models;
using Newtonsoft.Json;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static List<Publication> publications = new List<Publication>();

        static void Main(string[] args)
        {

            var identifier = "";

            if (args.Length == 0)
            {
                Console.WriteLine("Enter publisher id:");
                identifier = Console.ReadLine();
            }
            else
            {
                identifier = args[0];
                Console.WriteLine($"Publisher id: {identifier}");
            }

            var generator = new Generator(identifier);
            publications = generator.Generate();

            var factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange:exchangeAgent, type: "direct");
                    foreach(var publication in publications)
                    {
                        SendToQueue(channel,publication);
                    }
                }
            }

            Console.ReadLine();
        }
        private static void SendToQueue(IModel channel, Publication publication)
        {
            string json = JsonConvert.SerializeObject(publication);
            var bytes = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: exchangeAgent, routingKey: "", basicProperties: null, body: bytes);
            Console.WriteLine($"Sent {publication} on the queue for publications.");
        }
    }
}
