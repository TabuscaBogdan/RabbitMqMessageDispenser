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
        private string brokerExchangeAgent;
        private string hostName;
        private string consumerIdentifier = "";

        public Subscriptions(string brokerExchangeAgent, string hostName, string consumerIdentifier)
        {
            this.brokerExchangeAgent = brokerExchangeAgent + "Subscriptions";
            this.hostName = hostName;
            this.consumerIdentifier = consumerIdentifier;
        }

        private IModel OpenChannelOnBrokerSubscriptions(IConnection connection, string agent)
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: agent, type: "direct");

            return channel;
        }

        public void SetUpSendQueue(IModel channel)
        {
            channel.ExchangeDeclare(brokerExchangeAgent, type: "direct");
        }

        public void SendToQueue(IModel channel, Subscription subscription, string agent, string binding)
        {
            var byteMessage = Serialization.SerializeAndGetBytes(subscription);
            channel.BasicPublish(exchange: agent, routingKey: binding, basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent subscriptions: {subscription}");
        }

        public void SendSubscriptions()
        {
            var factory = new ConnectionFactory() { HostName = hostName };
            var binding = "";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = OpenChannelOnBrokerSubscriptions(connection, brokerExchangeAgent))
                {
                    SetUpSendQueue(channel);
                    var subscriptions = GetSubscriptions(consumerIdentifier);

                    foreach (var sub in subscriptions)
                    {
                        SendToQueue(channel, sub, brokerExchangeAgent, binding);
                    }
                }
            }
            Console.ReadLine();
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
                    SenderId = $"C{consumerId}:"
                };
                subs.Add(s);
            }
            return subs;
        }
    }
}
