using System;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;
using Utils;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static List<string> publications = new List<string>();

        static void Main(string[] args)
        {

            Console.WriteLine("Enter a generator identifier:");
            var identifier = "";

            if (args.Length == 0)
            {
                identifier = Console.ReadLine();
            }
            else
            {
                identifier = args[0];
            }

            var generator = new Generator(identifier);
            publications = generator.Generate();

            var factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange:exchangeAgent, type: "direct");
                    foreach(string publication in publications)
                    {
                        SendToQueue(channel,publication);
                    }
                }
            }

            Console.ReadLine();
        }
        private static void SendToQueue(IModel channel, string message)
        {
            var byteMessage = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeAgent, routingKey: "", basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent {message} on the queue for publications.");
        }
    }
}
