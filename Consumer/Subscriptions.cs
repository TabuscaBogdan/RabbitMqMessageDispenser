using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;
using Utils.Models;

namespace Consumer
{
    public class Subscriptions
    {
        private string brokerId;
        private string brokerSubscriptionsQueueName;
        private string consumerIdentifier = "";

        public Subscriptions(string brokerId, string consumerIdentifier)
        {
            this.brokerId = brokerId;
            brokerSubscriptionsQueueName = $"Subscriptions_{brokerId}";
            this.consumerIdentifier = consumerIdentifier;
        }
        
        public void SendSubscriptions()
        {
            var factory = RabbitFactory.GetFactory();

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: brokerSubscriptionsQueueName, type: "direct");
                    var subscriptions = GetSubscriptions(consumerIdentifier);

                    foreach (var sub in subscriptions)
                    {
                        SendToQueue(channel, sub);
                    }
                }
            }
            Console.ReadLine();
        }


        public void SendToQueue(IModel channel, Subscription subscription)
        {
            var byteMessage = ProtoSerialization.SerializeAndGetBytes(subscription);
            channel.BasicPublish(exchange: brokerSubscriptionsQueueName, routingKey: "", basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent subscriptions: {subscription}");
        }

        public List<Subscription> GetSubscriptions(string consumerId)
        {
            var fileName = String.Format(Constants.SubscriptionsPath, consumerId);
            return ReadSubscriptionsFromFile(consumerId, fileName);
        }

        public List<Subscription> ReadSubscriptionsFromFile(string consumerId, string fileName)
        {
            var subs = new List<Subscription>();
            string[] lines = FileReader.ReadAllLines(fileName);

            foreach (string line in lines)
            {
                var s = new Subscription
                {
                    Id = Guid.NewGuid().ToString(),
                    Filter = line.Replace("\0", ""),
                    SenderId = $"C{consumerId}",
                    ForwardNumber = 0
                };
                subs.Add(s);
            }
            return subs;
        }
    }
}
