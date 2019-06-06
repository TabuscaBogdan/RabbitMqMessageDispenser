using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Utils;

namespace Broker
{
    public static class Program
    {
        private static readonly string exchangeAgentPublishBroker = "publications";
        private static string exchangeAgentBrokerConsumer = "";
        private static int brokerNumber = 0;
        private static string receiverQueueName = "";

        public static Dictionary<string, List<string>> subscriptions = new Dictionary<string, List<string>>();
        public static List<string> publications = new List<string>();


        private static void Broker(IModel channelReceiver, IModel channelSender)
        {

            var consumer = new EventingBasicConsumer(channelReceiver);
            consumer.Received += (model, eventArguments) =>
            {
                var body = eventArguments.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = eventArguments.RoutingKey;
                Console.WriteLine(" [x] Received publication'{0}'", message);

                string publication = message.Split(":")[1];

                var receivers = FilterMessageBasedOnSubscriptions(publication);

                foreach (var receiver in receivers)
                {
                    SendToQueue(channelSender, message, exchangeAgentBrokerConsumer, receiver);
                }

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

        //Matching algorithm between publications and subscriptions
        public static HashSet<string> FilterMessageBasedOnSubscriptions(string message)
        {
            string pub = message.Replace("\0", "");
            string publication = pub.Substring(1, pub.Length - 2);
            string[] fieldsPublication = publication.Split(';');

            HashSet<string> receivers = new HashSet<string>();

            foreach (var receiver in subscriptions.Keys)
            {

                foreach (var sub in subscriptions[receiver])
                {
                    string subscription = sub.Substring(1, sub.Length - 2);
                    string[] fieldsSub = subscription.Split(';');
                    int sizeSub = fieldsSub.Length;
                    int index = 0;
                    foreach (string fieldSub in fieldsSub)
                    {
                        foreach (string fieldPub in fieldsPublication)
                        {
                            string topicSub = fieldSub.Split(',')[0];
                            string topicPub = fieldPub.Split(',')[0];
                            string valueSub = fieldSub.Split(',')[2];
                            string valuePub = fieldPub.Split(',')[1];
                            string operatorSub = fieldSub.Split(',')[1];
                            if (topicSub.Equals(topicPub))
                            {
                                switch (topicSub.Substring(1))
                                {
                                    case "patient-name":
                                    case "eye-color":
                                        if (valueSub.Equals(valuePub))
                                        {
                                            index = index + 1;
                                        }

                                        break;
                                    case "DoB":
                                        string valPub = valuePub.Substring(1, valuePub.Length - 3);
                                        string valSub = valueSub.Substring(1, valueSub.Length - 3);
                                        if (compareDoB(operatorSub, valPub, valSub))
                                        {
                                            index = index + 1;
                                        }
                                        break;
                                    case "height":
                                    case "heart-rate":
                                        string a = valuePub.Substring(0, valuePub.Length - 1);
                                        string b = valueSub.Substring(0, valueSub.Length - 1);
                                        if (Operator(operatorSub, Convert.ToDouble(a), Convert.ToDouble(b)))
                                        {
                                            index = index + 1;
                                        }

                                        break;
                                }

                            }

                        }
                    }
                    if (index == sizeSub)
                    {
                        receivers.Add(receiver);
                    }

                }
            }

            return receivers;
        }

        public static bool compareDoB(string op, string date1, string date2)
        {
            String format = "dd/MM/yyyy";
            string date1S = date1.Replace(".", "/");
            string date2S = date2.Replace(".", "/");
            switch (op)
            {
                case ">": return DateTime.ParseExact(date1S, format, null) > DateTime.ParseExact(date2S, format, null);
                case "<": return DateTime.ParseExact(date1S, format, null) < DateTime.ParseExact(date2S, format, null);
                case "==": return DateTime.ParseExact(date1S, format, null) == DateTime.ParseExact(date2S, format, null);
                case ">=": return DateTime.ParseExact(date1S, format, null) >= DateTime.ParseExact(date2S, format, null);
                case "<=": return DateTime.ParseExact(date1S, format, null) <= DateTime.ParseExact(date2S, format, null);
            }

            return false;
        }

        public static Boolean Operator(this string logic, Double x, Double y)
        {
            switch (logic)
            {
                case ">": return x > y;
                case "<": return x < y;
                case "==": return x == y;
                case ">=": return x >= y;
                case "<=": return x <= y;
                default: throw new Exception("invalid logic");
            }
        }



        public static void Main(string[] args)
        {
            Console.WriteLine("EnterBrokerNumber:");

            if(args.Length ==0)
            {
                var bnumber = Console.ReadLine();
                int.TryParse(bnumber, out brokerNumber);
            }
            else
            {
                int.TryParse(args[0], out brokerNumber);
            }

            Console.WriteLine($"Broker {brokerNumber} is up and running.");

            Subscriptions.brokerIdentifier = $"B{brokerNumber}";
            Subscriptions.brokerExchangeAgentSubscriptions = $"B{brokerNumber}Subscriptions";
            Subscriptions.hostName = Constants.RabbitMqServerAddress;


            var subFeedThreadReference = new ThreadStart(Subscriptions.ReceiveSubscriptions);
            Thread subFeedThread = new Thread(subFeedThreadReference);
            subFeedThread.Start();


            var factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
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
