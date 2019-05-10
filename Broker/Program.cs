using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Broker
{
    class Program
    {
        private static readonly string exchangeQueue = "publications";

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

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeQueue,
                    type: "direct");

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName,exchangeQueue,routingKey:"");

                Console.WriteLine(" [*] Waiting for messages.");

                ReceiveFromQueue(channel, queueName);

                Console.ReadLine();
            }
        }
    }
}
