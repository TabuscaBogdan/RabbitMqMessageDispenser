using System;
using System.Text;
using RabbitMQ.Client;
using System.Collections.Generic;
using Utils;
using Utils.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Publisher
{
    class Program
    {
        private static readonly string exchangeAgent = "publications";
        private static List<Publication> publications = new List<Publication>();

        static void Main(string[] args)
        {

            var publisherId = "";

            if (args.Length == 0)
            {
                Logger.Log("Enter publisher id:", true);
                publisherId = Console.ReadLine();
            }
            else
            {
                publisherId = args[0];
                Logger.Log($"Publisher id: {publisherId}", true);
            }


            try
            {
                var generator = new Generator(publisherId);
                publications = generator.Generate();
                var factory = RabbitFactory.GetFactory();
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: exchangeAgent, durable: true, exclusive: false, autoDelete: false, arguments: null);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        var i = 0;
                        //while (i < 100)
                        {
                            foreach (var publication in publications)
                            {
                                SendToQueue(channel, properties, publication);
                            }
                            i++;
                            // Logger.Log($"Publisher P{publisherId} sent {i} times.", true);

                            // Task.Delay(new TimeSpan(0, 5, 0));
                        }
                        sw.Stop();
                        Logger.Log($"Publisher P{publisherId} sent all publications.", true);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                Console.ReadLine();
            }
            Console.ReadLine();
        }
        private static void SendToQueue(IModel channel, IBasicProperties properties, Publication publication)
        {
            var bytes = ProtoSerialization.SerializeAndGetBytes(publication);
            channel.BasicPublish(exchange: "", routingKey: exchangeAgent, basicProperties: properties, body: bytes);
            Logger.Log($" [*] Sent publication: {publication} |");
        }
    }
}
