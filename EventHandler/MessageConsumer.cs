using AtiQxMessageBroker.Enums;
using AtiQxMessageBroker.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace AtiQxMessageBroker.EventHandler
{
    /// <summary>
    /// Abstract Event handler default class that imp the AtiQx IEventhandler interface.<br></br>
    /// It also Extends the RabbitMQ.Client.DefaultBasicConsumer, so implementations of this class can be used as RabbitMQ message consumers. <br></br>
    /// Create a subclass from this class that overrides the abstract methods and pass it as an argument to instantiate a MessageBroker Object. 
    /// </summary>
    public class MessageConsumer : DefaultBasicConsumer
    {
        protected IModel _channel;
        public IEventHandler eventHandler;

        public MessageConsumer(IModel channel, IEventHandler eventHandler)
        {
            _channel = channel;
            this.eventHandler = eventHandler;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            Dictionary<string, object> headers = (Dictionary<string, object>)properties.Headers;

            Guid eventId = Guid.Parse(properties.MessageId);
            DateTime timeStamp = AmqpTimestampToDateTime(properties.Timestamp);
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), properties.Type);
            string action = GetMessageHeaderValue("action", headers);
            string companyId = GetMessageHeaderValue("companyid", headers);
            ApplicationType application = (ApplicationType)Enum.Parse(typeof(ApplicationType), properties.AppId);
            bool persistent = properties.Persistent;
            Dictionary<string, string> messageBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(body.ToArray()));

            IAtiQxMessage newMessage = new AtiQxBaseMessage(eventId, timeStamp, eventType, action, application, messageBody, routingKey, persistent, companyId);

            if (eventHandler.Handle(newMessage))
                _channel.BasicAck(deliveryTag, false);
            else
                _channel.BasicNack(deliveryTag, false, true);
        }

        private string GetMessageHeaderValue(string headerValue, Dictionary<string, object> headers)
        {
            return Encoding.UTF8.GetString((byte[])headers[headerValue]);
        }

        private DateTime AmqpTimestampToDateTime(AmqpTimestamp ats)
        {
            DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0);
            return epoch.AddSeconds(ats.UnixTime).ToLocalTime();
        }
    }
}
