using AtiQxMessageBroker.Enums;

namespace AtiQxMessageBroker.Messages
{
    /// <summary>
    /// Message interface used for all the messages shared through the message broker
    /// </summary>
    public interface IAtiQxMessage
    {
        /// <summary>
        /// unique EventId
        /// </summary>
        public Guid EventId { get; }
        /// <summary>
        /// Event Timestamp
        /// </summary>
        public DateTime TimeStamp { get; }
        /// <summary>
        /// The actual message data as a collection of KeyValue objects
        /// </summary>
        public Dictionary<string, string> MessageBody { get; set; }
        /// <summary>
        /// bool whether the message should survive a broker restart
        /// </summary>
        public bool Persistent { get; set; }
        /// <summary>
        /// routing key used to route the message to the right exchanges/queues
        /// </summary>
        public string Routingkey { get; }
        /// <summary>
        /// Tells the receiver what type of event it is
        /// </summary>
        public EventType EventType { get; }
        /// <summary>
        /// Source application the message came from
        /// </summary>
        public ApplicationType SourceApplication { get; }
        /// <summary>
        /// action that tells what action happend. e.g. created, deleted, addressChanged etc..
        /// </summary>
        public string Action { get; }
        /// <summary>
        /// companyId the message is from. only the multi-tenant applications have to give this as a parameter
        /// </summary>
        public string CompanyId { get; }

        /// <summary>
        /// return all the header fields as a Dictionary<string, object> used as headers for the message.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetHeaders();
        /// <summary>
        /// returns the message body as a byte array so it can be send to the message broker via AMQP protocol.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessageBody();
    }
}
