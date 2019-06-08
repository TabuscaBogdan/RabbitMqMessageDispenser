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


            try
            {
                var generator = new Generator(identifier);
                publications = generator.Generate();
                var factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: exchangeAgent, durable: true, exclusive: false, autoDelete: false, arguments: null);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;


                        foreach (var publication in publications)
                        {
                            SendToQueue(channel, properties, publication);
                        }
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
            Console.WriteLine($" [*] Sent publication: {publication} |");
        }
    }
}
