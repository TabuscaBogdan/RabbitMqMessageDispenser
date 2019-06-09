using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Utils;
using Utils.Models;

namespace Broker
{
    public static class Program
    {
        private static readonly string publicationsQueueName = "publications";
        private static readonly string forwardPublicationsExchange = "publicationsforwarded";
        private static int brokerId;
        private static string receiverQueueName = "";

        public static Dictionary<string, List<Subscription>> RecieverSubscriptionsMap = new Dictionary<string, List<Subscription>>();
        public static Dictionary<string, Subscription> SubscriptionsMap = new Dictionary<string, Subscription>();
        public static ConnectionFactory factory = new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };


        static IModel ChanelPublicationsForward;
        static IModel ChanelPublicationsConsumer;
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("EnterBrokerNumber:");
                var bnumber = Console.ReadLine();
                int.TryParse(bnumber, out brokerId);
            }
            else
            {
                int.TryParse(args[0], out brokerId);
            }

            Console.WriteLine($"Broker B{brokerId} is up and running.");
            var s = new Subscriptions(brokerId);

            var subFeedThreadReference = new ThreadStart(s.ReceiveSubscriptions);
            Thread subFeedThread = new Thread(subFeedThreadReference);
            subFeedThread.Start();

            using (var connection = factory.CreateConnection())
            {
                var channelRecievePublications = connection.CreateModel();
                channelRecievePublications.QueueDeclare(queue: publicationsQueueName,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
                channelRecievePublications.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                ChanelPublicationsConsumer = connection.CreateModel();

                Broker(channelRecievePublications, ChanelPublicationsConsumer);
                DeclareForwardPublicationsExchange(connection);
                Console.ReadLine();

                ChanelPublicationsForward.Dispose();
                ChanelPublicationsConsumer.Dispose();

            }


        }
        private static void DeclareForwardPublicationsExchange(IConnection connection)
        {
            ChanelPublicationsForward = connection.CreateModel();
            ChanelPublicationsForward.ExchangeDeclare(exchange: forwardPublicationsExchange,
                                        type: "direct");
            var queueName = ChanelPublicationsForward.QueueDeclare().QueueName;
            ChanelPublicationsForward.QueueBind(queue: queueName,
                                  exchange: forwardPublicationsExchange,
                                  routingKey: $"B{brokerId}");
            var consumer = new EventingBasicConsumer(ChanelPublicationsForward);
            consumer.Received += ConsumeForwardedPublications;
            ChanelPublicationsForward.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
        }
        private static void ConsumeForwardedPublications(object sender, BasicDeliverEventArgs e)
        {
            var publication = ProtoSerialization.Deserialize<Publication>(e.Body);
            Console.WriteLine($" [x] Received forwarded publication on routing key {e.RoutingKey}");
            Console.WriteLine($" [x] Received forwarded publication {publication}");
            if (SubscriptionsMap[publication.SubscriptionMatchId].SenderId.StartsWith('B'))
            {
                SendForwardsPublication(publication, SubscriptionsMap[publication.SubscriptionMatchId].SenderId);
            }
            else
            {
                SendToConsumerQueue(ChanelPublicationsConsumer, publication, SubscriptionsMap[publication.SubscriptionMatchId].SenderId);
            }

        }
        private static void SendForwardsPublication(Publication publication, string receiverId)
        {
            var body = ProtoSerialization.SerializeAndGetBytes(publication);
            ChanelPublicationsForward.BasicPublish(exchange: forwardPublicationsExchange,
                                 routingKey: receiverId,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($" [x] Sent forward publication to {receiverId} : {publication}");
        }
        private static void Broker(IModel channelReceiver, IModel channelSender)
        {
            var consumer = new EventingBasicConsumer(channelReceiver);
            consumer.Received += (model, eventArguments) =>
            {
                var body = eventArguments.Body;
                var publication = ProtoSerialization.Deserialize<Publication>(body);
                Console.WriteLine($" [x] Received publication {publication} ");

                var matchedSubscriptions = FilterMessageBasedOnSubscriptions(publication);

                foreach (var subscriptions in matchedSubscriptions)
                {
                    publication.SubscriptionMatchId = subscriptions.Id;
                    if (subscriptions.SenderId.StartsWith('C'))
                    {
                        SendToConsumerQueue(channelSender, publication, subscriptions.SenderId);
                    }
                    if (subscriptions.SenderId.StartsWith('B'))
                    {
                        SendForwardsPublication(publication, subscriptions.SenderId);
                    }
                }
            };
            channelReceiver.BasicConsume(queue: receiverQueueName,
                autoAck: true,
                consumer: consumer);

        }


        private static void SendToConsumerQueue(IModel channel, Publication publication, string receiverId)
        {
            Console.WriteLine($" [*] Send to consumer {publication}");

            var bytes = ProtoSerialization.SerializeAndGetBytes(publication);
            channel.QueueDeclare(queue: receiverId, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            channel.BasicPublish(exchange: "", routingKey: receiverId, basicProperties: properties, body: bytes);
        }

        public static List<Subscription> FilterMessageBasedOnSubscriptions(Publication p)
        {
            string pub = p.Contents.Replace("\0", "");
            string publication = pub.Substring(1, pub.Length - 2);
            string[] fieldsPublication = publication.Split(';');

            List<Subscription> matchedSubscriptions = new List<Subscription>();

            foreach (var receiver in RecieverSubscriptionsMap.Keys)
            {
                foreach (Subscription sub in RecieverSubscriptionsMap[receiver])
                {
                    string subscription = sub.Filter.Substring(1, sub.Filter.Length - 2);
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
                                        if (CompareDoB(operatorSub, valPub, valSub))
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
                        matchedSubscriptions.Add(sub);
                    }

                }
            }

            return matchedSubscriptions;
        }

        public static bool CompareDoB(string op, string date1, string date2)
        {
            String format = "dd/MM/yyyy";
            string date1S = date1.Replace(".", "/");
            string date2S = date2.Replace(".", "/");
            var dateTime1 = DateTime.ParseExact(date1S, format, null);
            var dateTime2 = DateTime.ParseExact(date2S, format, null);
            switch (op)
            {
                case ">": return dateTime1 > dateTime2;
                case "<": return dateTime1 < dateTime2;
                case "==": return dateTime1 == dateTime2;
                case ">=": return dateTime1 >= dateTime2;
                case "<=": return dateTime1 <= dateTime2;
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


    }
}
