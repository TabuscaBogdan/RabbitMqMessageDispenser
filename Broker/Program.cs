using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
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

        public static Dictionary<string,List<string>> subscriptions;


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
                var receivers = FilterMessageBasedOnSubscriptions(message);

                foreach(var receiver in receivers)
                {
                    SendToQueue(channelSender, message, exchangeAgentBrokerConsumer, receiver);
                }
                //

            };
            channelReceiver.BasicConsume(queue: receiverQueueName,
                autoAck: true,
                consumer: consumer);

        }

        private static void SetUpSendQueue(IModel channel)
        {
            exchangeAgentBrokerConsumer = "B" + brokerNumber;
            channel.ExchangeDeclare(exchangeAgentBrokerConsumer, type: "direct");
        }

        public static void SetUpReceiveQueue(IModel channel, string agent)
        {
            channel.ExchangeDeclare(exchange: agent,
                type: "direct");

            receiverQueueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(receiverQueueName, agent, routingKey: "");

        }

        private static void SendToQueue(IModel channel, string message, string agent, string binding)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: agent, routingKey: binding, basicProperties: null, body: encodedMessage);
        }

        //TODO: Make Propper Filtering!
        public static HashSet<string> FilterMessageBasedOnSubscriptions(string message)
        {

            int publication;
            int.TryParse(message.Split(':')[1], out publication);

            HashSet<string> receivers = new HashSet<string>();

            foreach(var receiver in subscriptions.Keys)
            {
                foreach(var sub in subscriptions[receiver])
                {

                    //TODO: needs work for interpreting subscriptions
                    if(publication%2==0 && sub == "par")
                    {
                        receivers.Add(receiver);
                    }
                    if(publication%2==1 && sub == "impar")
                    {
                        receivers.Add(receiver);
                    }
                }
            }

            return receivers;
        }



        public static void Main(string[] args)
        {
            subscriptions = new Dictionary<string, List<string>>();

            Console.WriteLine("EnterBrokerNumber:");
            var bnumber = Console.ReadLine();
            int.TryParse(bnumber, out brokerNumber);

            Console.WriteLine($"Broker {brokerNumber} is up and running.");

            Subscriptions.brokerIdentifier = $"B{brokerNumber}";
            Subscriptions.brokerExchangeAgentSubscriptions = $"B{brokerNumber}Subscriptions";
            Subscriptions.hostName = "localhost";

            
            var subFeedThreadReference = new ThreadStart(Subscriptions.ReceiveSubscriptions);
            Thread subFeedThread = new Thread(subFeedThreadReference);
            subFeedThread.Start();

            
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                var channelReceiver = connection.CreateModel();
                var channelSender = connection.CreateModel();

                SetUpReceiveQueue(channelReceiver, exchangeAgentPublishBroker);
                SetUpSendQueue(channelSender);

                Broker(channelReceiver, channelSender);

                Console.ReadLine();
            }
            
        }
    }
}
