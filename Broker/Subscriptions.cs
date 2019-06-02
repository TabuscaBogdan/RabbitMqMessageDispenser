using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Broker
{
    public static class Subscriptions
    {
        public static string brokerExchangeAgentSubscriptions;
        public static string hostName;
        public static string brokerIdentifier;
        public static string queueName = "";


        public static void ReceiveSubscriptions()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
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
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine($"Rceived subscription {message}");

                    var subscriberID = message.Split(':')[0];
                    var subscription = message.Split(':')[1];

                    if (Program.subscriptions.ContainsKey(subscriberID))
                    {
                        Program.subscriptions[subscriberID].Add(subscription);
                    }
                    else
                    {
                        Program.subscriptions[subscriberID] = new List<string>();
                        Program.subscriptions[subscriberID].Add(subscription);
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
