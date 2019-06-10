using System;

namespace Utils
{
    public static class Constants
    {
        public static readonly String RabbitMqServerAddress = "localhost";
        public static readonly String PublicationsFileName = "Resources/test_10000/publications_P{0}.txt";
        public static readonly String SubscriptionsPath = "Resources/test_10000/subscriptions_C{0}.txt";
        public static readonly int NumberOfBrokers = 3;
    }
}
