using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public static class RabbitFactory
    {
        public static ConnectionFactory GetFactory()
        {
            if (Constants.RunLocal)
            {
                return new ConnectionFactory() { HostName = Constants.RabbitMqServerAddress };
            }
            return new ConnectionFactory() { Uri = Constants.AmqpServerAddress };
        }

    }
}
