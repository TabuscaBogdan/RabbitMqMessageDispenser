using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Utils;
using Utils.Models;

namespace Consumer
{
    class Program
    {
        private static string brokerId = "B";
        private static string consumerId = "";
        private static string hostName = Constants.RabbitMqServerAddress;
        private static decimal averageLatencyTime = 0;
        private static int messageCount = 0;
        private static List<decimal> Latencies = new List<decimal>();


        static void Main(string[] args)
        {
            var factory = RabbitFactory.GetFactory();
            if (args.Length == 0)
            {
                //Console.WriteLine("Enter a consumer ID:");
                consumerId = Console.ReadLine();

                //Console.WriteLine("Enter a broker ID (if random leave empty):");
                var broker = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(broker))
                {
                    brokerId += GetRandomBroker();
                }
                else
                {
                    brokerId += broker;
                }
                Console.WriteLine($"Broker ID: {brokerId}");
            }
            else
            {
                consumerId = args[0];
                Console.WriteLine($"Consumer ID: {consumerId}");

                if (args.Length == 1)
                {
                    brokerId += GetRandomBroker();
                }
                else
                {
                    brokerId += args[1];
                }
                //Console.WriteLine($"Broker ID: {brokerId}");
            }
            var publicationsQueueName = $"C{consumerId}";


            Subscriptions sub = new Subscriptions(brokerId, consumerId);

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
                        var publication = ProtoSerialization.Deserialize<Publication>(body);
                        var latency = GetLatency(publication);
                        Latencies.Add(latency);
                        // UpdateAverageLatency(latency);
                        // var routingKey = eventArguments.RoutingKey;
                        Console.WriteLine($" [*] Received publication {publication}\n Latency:{latency}");
                        // ShowAverageLatency();
                    };
                    channel.BasicConsume(queue: publicationsQueueName,
                        autoAck: true,
                        consumer: consumer);
                    Console.WriteLine("Awaiting Messages...");

                    Console.ReadLine();
                    Console.WriteLine("Writing to file latency results...");
                    CSVFileWriter f = new CSVFileWriter(consumerId, latency: Latencies);
                    f.WriteAllLatenciesInCSV();
                    Console.WriteLine("Finishd...");
                }
            }
        }

        private static decimal GetLatency(Publication publication)
        {
            DateTime timeOfArrival = DateTime.UtcNow;
            DateTime timeOfCreation = publication.Timestamp;

            TimeSpan difference = timeOfArrival - timeOfCreation;

            decimal latency = (decimal)difference.TotalSeconds;

            return latency;
        }

        private static void UpdateAverageLatency(decimal latency)
        {
            averageLatencyTime += latency;
            messageCount++;
        }

        private static void ShowAverageLatency()
        {
            Console.WriteLine($"Average latency of messages: {averageLatencyTime/messageCount}");
        }

        private static string GetRandomBroker()
        {
            Random r = new Random();
            return r.Next(1, Constants.NumberOfBrokers).ToString();
        }
    }
}
