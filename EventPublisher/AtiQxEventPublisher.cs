using AtiQxMessageBroker.Enums;
using AtiQxMessageBroker.MessageBrokerClient;
using AtiQxMessageBroker.Messages;
using RabbitMQ.Client;

namespace AtiQxMessageBroker.EventPublisher
{
    internal class AtiQxEventPublisher : IAtiQxEventPublisher
    {
        private readonly IModel _channel;
        private readonly ApplicationType _application;
        private readonly string CompanyId;

        public AtiQxEventPublisher(MessageBrokerClientConfig messageBrokerConfig)
        {
            _application = messageBrokerConfig.Application;
            CompanyId = messageBrokerConfig.CompanyId;
            var connectionFactory = new ConnectionFactory()
            {
                UserName = messageBrokerConfig.Username,
                Password = messageBrokerConfig.Password,
                HostName = messageBrokerConfig.Host
            };
            var connection = connectionFactory.CreateConnection();
            _channel = connection.CreateModel();
        }

        public void Publish(IAtiQxMessage message)
        {
            if ((_application == ApplicationType.guest || _application == ApplicationType.atiqxid) && string.IsNullOrEmpty(message.CompanyId))
                throw new Exception("Messages from multi-tenant application {0} require a companyID when being send");
            if (message.MessageBody.Count == 0)
                throw new Exception("Message body cannot be empty");

            byte[] body = message.GetMessageBody();
            var properties = _channel.CreateBasicProperties();
            properties.AppId = _application.ToString();
            properties.MessageId = message.EventId.ToString();
            properties.Type = message.EventType.ToString();
            properties.Timestamp = GetAmqpTimestamp(message.TimeStamp);
            properties.Persistent = message.Persistent;
            properties.Headers = message.GetHeaders();
            string routingKey;

            //if the current application is one of the multitenant applications, the message will be sent to the multi-tenant exchange instead of the client specific exchange.
            //it also means that the companyID is expected to be set within the message.
            string exchangeName;
            if (_application == ApplicationType.guest || _application == ApplicationType.atiqxid)
            {
                exchangeName = _application.ToString();
                routingKey = string.Format("{0}.{1}.{2}", message.CompanyId, _application, message.Routingkey);
                properties.Headers.Add("companyid", message.CompanyId);

            }
            else
            {
                exchangeName = CompanyId;
                routingKey = string.Format("{0}.{1}.{2}", CompanyId, _application, message.Routingkey);
                properties.Headers.Add("companyid", CompanyId);
            }

            _channel.BasicPublish(exchangeName, routingKey, properties, body);
        }

        private AmqpTimestamp GetAmqpTimestamp(DateTime datetime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixTime = (datetime.ToUniversalTime() - epoch).TotalSeconds;
            var timestamp = new AmqpTimestamp((long)unixTime);
            return timestamp;
        }
    }
}
