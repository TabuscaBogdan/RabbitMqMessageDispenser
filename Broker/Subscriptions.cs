using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;
using Utils.Models;

namespace Broker
{
    public static class Subscriptions
    {
        public static string brokerExchangeAgentSubscriptions;
        public static string hostName;
        public static string brokerIdentifier;
        public static string queueName = "";
        public static List<Subscription> receivedSubscriptions = new List<Subscription>();


        public static void ReceiveSubscriptions()
        {
            var factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: brokerExchangeAgentSubscriptions,
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;


                channel.QueueBind(queue: queueName,
                                  exchange: brokerExchangeAgentSubscriptions,
                                  routingKey: "");


                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var message = Serialization.Deserialize<Subscription>(ea.Body);
                    receivedSubscriptions.Add(message);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine($"Received subscription {message}");

                    if (Program.subscriptions.ContainsKey(message.SenderId))
                    {
                        Program.subscriptions[message.SenderId].Add(message);
                    }
                    else
                    {
                        Program.subscriptions[message.SenderId] = new List<Subscription>() { message };
                    }
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine("Waiting...");
                Console.ReadLine();
            }
        }
    }
}
