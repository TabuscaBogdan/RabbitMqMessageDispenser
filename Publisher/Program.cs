using System;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static List<string> pub = new List<string>();
        private static void SendToQueue(IModel channel, string message)
        {
            var byteMessage = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeAgent, routingKey: "", basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent {message} on the queue for publications.");
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            Console.WriteLine("Enter a generator identifier:");
            var identifier = Console.ReadLine();
            var generator = new Generator(10,identifier);
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange:exchangeAgent, type: "direct");
                    pub = generator.Generate(identifier);
                    foreach(string publication in pub)
                    {
                        SendToQueue(channel,publication);
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
