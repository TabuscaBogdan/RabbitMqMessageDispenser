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
            Console.WriteLine($"Sent {message} on the queue for subscriptions. Agent:{agent} RotingKey: {""}.");
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


        //------------------------------
        //TODO Ecaterina Mihaela
        //trebuie facute in asa fel incat brokerul sa primeasca o lista de cv de genu C1:{categorie}<conditie>,{categorie}<conditie>...
        //citire din fisier sau de la mana sau vei tu cum
        //trebuie sa lucrezi in stransa legatura cu cine implementeaza filtrul pe broker (Madalina Gabriela)

        public List<string> GetSubscriptions(string consumerId)
        {
            var subs = new List<string>();

            if(consumerId=="1")
            {
                subs.Add($"C{consumerId}:impar");
            }
            else
            {
                subs.Add($"C{consumerId}:par");
            }
            return subs;
        }
    }
}
