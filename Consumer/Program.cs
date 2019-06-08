using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using Utils;
using Utils.Models;

namespace Consumer
{
    class Program
    {
        private static string brokerId = "B";
        private static string consumerId = "";
        private static string hostName = Constants.RabbitMqServerAddress;

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = hostName };
            if (args.Length == 0)
            {
                Console.WriteLine("Enter a consumer ID:");
                consumerId = Console.ReadLine();

                Console.WriteLine("Enter a broker ID:");
                brokerId += Console.ReadLine();
            }
            else
            {
                consumerId = args[0];
                brokerId += args[1];
                Console.WriteLine($"Consumer ID:{consumerId}");
                Console.WriteLine($"Broker ID:{brokerId}");
            }
            var publicationsQueueName = $"C{consumerId}";


            Subscriptions sub = new Subscriptions(brokerId, hostName, consumerId);

            var subFeedThreadReference = new ThreadStart(sub.SendSubscriptions);
            Thread subFeedThread = new Thread(subFeedThreadReference);
            subFeedThread.Start();


            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: publicationsQueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, eventArguments) =>
                    {
                        var body = eventArguments.Body;
                        var publication = Serialization.Deserialize<Publication>(body);
                        var routingKey = eventArguments.RoutingKey;
                        Console.WriteLine($" [*] Received publication {publication}");
                    };
                    channel.BasicConsume(queue: publicationsQueueName,
                        autoAck: true,
                        consumer: consumer);
                    Console.WriteLine("Awaiting Messages...");
                    Console.ReadLine();
                }
            }
        }

        public static IModel OpenChannelOnBroker(IConnection connection, ref string queueName, string agent, string binding)
        {
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: agent, type: "direct");
            queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName, exchange: brokerId, routingKey: binding);

            return channel;
        }
    }
}
