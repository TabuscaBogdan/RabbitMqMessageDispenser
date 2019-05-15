using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Consumer
{
    class Program
    {
        private static string exchangeAgent = "B1";

        private static IModel OpenChannelOnBroker(IConnection connection,ref string queueName)
        {
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange:exchangeAgent, type:"topic");
            queueName = channel.QueueDeclare().QueueName;

            return channel;
        }

        private static void BindTopicsToQueue(IModel channel, string queueName , List<string> bindings)
        {
            foreach (var binding in bindings)
            {
                channel.QueueBind(queue: queueName, exchange: exchangeAgent, routingKey: binding);
            }
        }

        private static string ReceiveFromQueue(IModel channel, string queueName)
        {
            string messageReceived = "";

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArguments) =>
            {
                var body = eventArguments.Body;
                var message = Encoding.UTF8.GetString(body);
                messageReceived = message;
                var routingKey = eventArguments.RoutingKey;
                Console.WriteLine(" [x] Received '{0}':'{1}'",
                    routingKey, message);
            };
            channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);

            return messageReceived;
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            var queueName = "";
            using (var connection = factory.CreateConnection())
            {
                using (var channel = OpenChannelOnBroker(connection, ref queueName))
                {
                    var topics=new List<string>();
                    topics.Add("");
                    //TODO add topics
                    BindTopicsToQueue(channel,queueName,topics);

                    Console.WriteLine("Awaiting Messages...");

                    ReceiveFromQueue(channel, queueName);

                    Console.ReadLine();
                }
            }
        }
    }
}
