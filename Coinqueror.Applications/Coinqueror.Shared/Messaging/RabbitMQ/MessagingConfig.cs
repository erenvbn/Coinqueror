using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinqueror.Shared.Messaging.RabbitMQ
{
    public class MessagingConfig
    {
        public string HostNameRMQ { get; set; } = "localhost";
        public int PortRMQ { get; set; } = 5672;
        public string ExchangeNameRMQ { get; set; } = "default_exchange";
        public string QueueNameRMQ { get; set; } = "default_queue";
        public string RoutingKeyRMQ { get; set; } = "default_routing_key";

        public IConnection GetRabbitMQConnection()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = HostNameRMQ;
            connectionFactory.Port = PortRMQ;
            connectionFactory.UserName = "guest";
            connectionFactory.Password = "guest";
            connectionFactory.ClientProperties.Add("ExchangeNameRMQ", "default_exchange");
            connectionFactory.ClientProperties.Add("QueueNameRMQ", "default_queue");
            connectionFactory.ClientProperties.Add("RoutingKeyRMQ", "default_routing_key");

            return connectionFactory.CreateConnectionAsync().Result;
        }

        //= _messagingConfig.GetRabbitMQConnection().Result;
    }

}
