using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinqueror.Shared.Messaging.RabbitMQ
{
    public class MessagePublisher
    {
        private readonly MessagingConfig _messagingConfig;
        private IConnection _connection;

        public MessagePublisher(MessagingConfig messagingConfig, IConnection connection)
        {
            _messagingConfig = messagingConfig;
            _connection = connection;
        }

        public async Task PublishMessageAsync(string message)
        {
            using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: _messagingConfig.ExchangeNameRMQ,
                type: "direct",
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: CancellationToken.None
            );

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: _messagingConfig.ExchangeNameRMQ,
                routingKey: _messagingConfig.RoutingKeyRMQ,
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: body,
                cancellationToken: CancellationToken.None
            );
        }
    }
}