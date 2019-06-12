using System;
using System.Collections.Generic;

namespace Utils
{
    public static class Constants
    {
        public static readonly String RabbitMqServerAddress = "localhost";
        public static readonly Uri AmqpServerAddress = new Uri("amqp://dguytdtm:zTOjLoU-cX1KcdMGKFDzD-_i3_e1Egrr@macaw.rmq.cloudamqp.com/dguytdtm");
        public static readonly String PublicationsFileName = "Resources/test_10000/publications_P{0}.txt";
        public static readonly String SubscriptionsPath = "Resources/test_10000/subscriptions_c{0}.txt";
        public static readonly int NumberOfBrokers = 1;
        public static readonly bool RunLocal = true;
        public static readonly bool Debug = true;

        public static readonly String LatencyOutputFileName = "Resources/LatencyOutput/Latencies{0}.csv";
    }
}
