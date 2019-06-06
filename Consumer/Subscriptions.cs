using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

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

        public void SendToQueue(IModel channel, string message, string agent, string binding)
        {
            var byteMessage = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: agent, routingKey: binding, basicProperties: null, body: byteMessage);
            Console.WriteLine($"Sent {message} on the queue for subscriptions.");
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

                    foreach(var sub in subscriptions)
                    {

                         SendToQueue(channel, sub, brokerExchangeAgent, binding);

                    }
                }
            }
            Console.ReadLine();
        }


        //the generated subscriptions are read from the file

        public List<string> GetSubscriptions(string consumerId)
        {
            var subs = new List<string>();
            var fileName = @"D:\Master\EBS\EBS\subscriptions_C"+ consumerId+".txt";
            subs = readFromFile(consumerId,fileName);
            return subs;
        }

        public List<string> readFromFile(string consumerId, string fileName){
        var subs = new List<string>();
        string[] lines = System.IO.File.ReadAllLines(fileName);

        foreach (string line in lines)
        {
            subs.Add($"C{consumerId}:"+line.Replace("\0", ""));
        }

            return subs;
        }
    }
}
