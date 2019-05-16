using System;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Broker
{
    class Program
    {
        private static readonly string exchangeAgentPublishBroker = "publications";
        private static string exchangeAgentBrokerConsumer = "";
        private static int brokerNumber=0;
        private static string receiverQueueName = "";


        private static void Broker(IModel channelReceiver, IModel channelSender)
        {

            var consumer = new EventingBasicConsumer(channelReceiver);
            consumer.Received += (model, eventArguments) =>
            {
                var body = eventArguments.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = eventArguments.RoutingKey;
                Console.WriteLine(" [x] Received '{0}':'{1}'",
                    routingKey, message);
                //
                SendToQueue(channelSender, message);
                //

            };
            channelReceiver.BasicConsume(queue: receiverQueueName,
                autoAck: true,
                consumer: consumer);

        }

        private static void SetUpSendQueue(IModel channel)
        {
            exchangeAgentBrokerConsumer = "B" + brokerNumber;
            channel.ExchangeDeclare(exchangeAgentBrokerConsumer, type: "topic");
        }

        private static void SetUpReceiveQueue(IModel channel)
        {
            channel.ExchangeDeclare(exchange: exchangeAgentPublishBroker,
                type: "direct");

            receiverQueueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(receiverQueueName, exchangeAgentPublishBroker, routingKey: "");

        }

        private static void SendToQueue(IModel channel, string message)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: exchangeAgentBrokerConsumer, routingKey: "", basicProperties: null, body: encodedMessage);
        }




        public static void Main(string[] args)
        {
            Console.WriteLine("EnterBrokerNumber:");
            var bnumber = Console.ReadLine();
            int.TryParse(bnumber, out brokerNumber);

            Console.WriteLine($"Broker {brokerNumber} is up and running.");

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                var channelReceiver = connection.CreateModel();
                var channelSender = connection.CreateModel();

                SetUpReceiveQueue(channelReceiver);
                SetUpSendQueue(channelSender);

                Broker(channelReceiver, channelSender);

                Console.ReadLine();
            }
        }
    }
}
