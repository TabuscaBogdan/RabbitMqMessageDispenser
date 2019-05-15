﻿using System;
using System.Text;
using RabbitMQ.Client;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static void SendToQueue(IModel channel, string message)
        {
            var byteMessage = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeAgent, routingKey: "", basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent {message} on the queue.");
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

                    string pub = generator.Generate();
                    while (pub!="")
                    {
                        SendToQueue(channel,pub);
                        pub = generator.Generate();
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
