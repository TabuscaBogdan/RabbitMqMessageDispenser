using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Threading;
using Utils;

namespace Consumer
{
    class Program
    {
        private static string exchangeAgent = "B";
        private static string consumerID = "";
        private static string hostName = Constants.RabbitMqServerAddress;

        public static IModel OpenChannelOnBroker(IConnection connection,ref string queueName,string agent, string binding)
        {
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange:agent, type:"direct");
            queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName, exchange: exchangeAgent, routingKey: binding);

            return channel;
        }

        /* // needed for topic
        private static void BindTopicsToQueue(IModel channel, string queueName , List<string> bindings)
        {
            foreach (var binding in bindings)
            {
                channel.QueueBind(queue: queueName, exchange: exchangeAgent, routingKey: binding);
            }
        }
        */
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
                Console.WriteLine(" [x] Received publication '{0}':'{1}'",
                    routingKey, message);
            };
            channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);

            return messageReceived;
        }

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = hostName};
            var queueName = "";
            if (args.Length == 0)
            {
                Console.WriteLine("Enter a consumer ID:");
                consumerID = Console.ReadLine();

                Console.WriteLine("Enter a broker ID:");
                exchangeAgent += Console.ReadLine();
            }
            else
            {

                consumerID = args[0];
                exchangeAgent += args[1];
                Console.WriteLine($"Consumer ID:{consumerID}");
                Console.WriteLine($"Broker ID:{exchangeAgent}");
            }


            Subscriptions sub = new Subscriptions(exchangeAgent, hostName, consumerID);

            var subFeedThreadReference = new ThreadStart(sub.SendSubscriptions);
            Thread subFeedThread = new Thread(subFeedThreadReference);
            subFeedThread.Start();


            using (var connection = factory.CreateConnection())
            {
                using (var channel = OpenChannelOnBroker(connection, ref queueName, exchangeAgent, $"C{consumerID}"))
                {

                    Console.WriteLine("Awaiting Messages...");

                    ReceiveFromQueue(channel, queueName);

                    Console.ReadLine();
                }
            }
        }
    }
}
