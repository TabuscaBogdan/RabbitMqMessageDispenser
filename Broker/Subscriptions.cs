using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Models;

namespace Broker
{
    public class Subscriptions
    {
        public string brokerExchangeAgentSubscriptions;
        public int brokerId;
        public int subscriptionNumber = 0;

        ConnectionFactory factory = RabbitFactory.GetFactory();
        public Subscriptions(int brokerId)
        {
            brokerExchangeAgentSubscriptions = $"Subscriptions_B{brokerId}";
            this.brokerId = brokerId;
        }

        public void ReceiveSubscriptions()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: brokerExchangeAgentSubscriptions,
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;


                channel.QueueBind(queue: queueName,
                                  exchange: brokerExchangeAgentSubscriptions,
                                  routingKey: "");


                Logger.Log(" [*] Waiting for subscriptions...", true);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += DealWithSubscriptions;
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                Console.ReadLine();
            }
        }

        private void DealWithSubscriptions(object model, BasicDeliverEventArgs ea)
        {
            var subscription = ProtoSerialization.Deserialize<Subscription>(ea.Body);
            Logger.Log($" [*] Received subscription {subscription}");
            Logger.Log($"S {++subscriptionNumber}", true);

            if (Program.RecieverSubscriptionsMap.ContainsKey(subscription.SenderId))
            {
                Program.RecieverSubscriptionsMap[subscription.SenderId].Add(subscription);
            }
            else
            {
                Program.RecieverSubscriptionsMap[subscription.SenderId] = new List<Subscription>() { subscription };
            }
            if (!Program.SubscriptionsMap.ContainsKey(subscription.Id))
            {
                Program.SubscriptionsMap.Add(subscription.Id, subscription);
            }
            ForwardSubscription(subscription);
        }

        public void ForwardSubscription(Subscription subscription)
        {
            if (subscription.ForwardNumber == (Constants.NumberOfBrokers - 1))
            {
                return;
            }
            string forwardId = GetNextBrokerId(subscription);
            var s = new Subscription
            {
                Id = subscription.Id,
                SenderId = $"B{brokerId}",
                Filter = subscription.Filter,
                ForwardNumber = subscription.ForwardNumber + 1
            };

            var brokerSubscriptionsQueueName = $"Subscriptions_{forwardId}";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: brokerSubscriptionsQueueName, type: "direct");

                    var byteMessage = ProtoSerialization.SerializeAndGetBytes(s);
                    channel.BasicPublish(exchange: brokerSubscriptionsQueueName, routingKey: "", basicProperties: null, body: byteMessage);
                    Logger.Log($" [*] Forwarded subscription: {s}");
                }
            }
        }

        private string GetNextBrokerId(Subscription subscription)
        {
            if (brokerId == Constants.NumberOfBrokers)
            {
                return "B1";
            }
            return $"B{brokerId + 1}";
        }
    }
}
