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
                Logger.Log("Enter a consumer ID:", true);
                consumerId = Console.ReadLine();

                Logger.Log("Enter a broker ID (if random leave empty):", true);
                var broker = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(broker))
                {
                    brokerId += GetRandomBroker();
                }
                else
                {
                    brokerId += broker;
                }
                Logger.Log($"Broker ID: {brokerId}", true);
            }
            else
            {
                consumerId = args[0];
                Logger.Log($"Consumer ID: {consumerId}", true);

                if (args.Length == 1)
                {
                    brokerId += GetRandomBroker();
                }
                else
                {
                    brokerId += args[1];
                }
                Logger.Log($"Broker ID: {brokerId}", true);
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
                    consumer.Received += ConsumePublications;
                    channel.BasicConsume(queue: publicationsQueueName,
                        autoAck: true,
                        consumer: consumer);
                    Logger.Log("Awaiting Messages...", true);

                    Console.ReadLine();
                    consumer.Received -= ConsumePublications;
                    Logger.Log("Writing to file latency results...");

                    CSVFileWriter f = new CSVFileWriter(consumerId, latency: Latencies);
                    f.WriteAllLatenciesInCSV();
                    Logger.Log("Finishd...");
                }
            }
        }

        private static void ConsumePublications(object model, BasicDeliverEventArgs eventArguments)
        {
            var body = eventArguments.Body;
            var publication = ProtoSerialization.Deserialize<Publication>(body);
            var latency = GetLatency(publication);
            Latencies.Add(latency);
            Logger.Log($" [*] Received publication {Latencies.Count}", true);

            // UpdateAverageLatency(latency);
            // var routingKey = eventArguments.RoutingKey;
            Logger.Log($" [*] Received publication {publication}\n Latency:{latency}");
            // ShowAverageLatency();
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
            Logger.Log($"Average latency of messages: {averageLatencyTime/messageCount}");
        }

        private static string GetRandomBroker()
        {
            Random r = new Random();
            return r.Next(1, Constants.NumberOfBrokers).ToString();
        }
    }
}
