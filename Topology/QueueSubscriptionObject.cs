using AtiQxMessageBroker.Enums;

namespace AtiQxMessageBroker.Topology
{
    /// <summary>
    /// Class that contains information that is needed to create and bind a queue
    /// </summary>
    public class QueueSubscriptionObject
    {
        /// <summary>
        /// EventType to subscribe to.
        /// </summary>
        public readonly EventType EventType;
        /// <summary>
        /// Source application of the event to subscribe to. since listening to external applications is forbidden, external will result in an error
        /// </summary>
        public readonly ApplicationType SourceApplication;
        /// <summary>
        /// When true, the queue will survive a broker restart.
        /// </summary>
        public readonly bool Durable;
        /// <summary>
        /// when true, queue that has had at least one consumer is deleted when last consumer unsubscribed.
        /// </summary>
        public readonly bool AutoDelete;
        /// <summary>
        /// used by only one connection, queue will be deleted when that connection closes.
        /// </summary>
        public readonly bool Exclusive;
        /// <summary>
        /// **
        /// </summary>
        public readonly Dictionary<string, object> Arguments;

        public QueueSubscriptionObject(EventType eventType, ApplicationType sourceApplication, bool durable = true, bool autoDelete = false,
            bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            if (sourceApplication == ApplicationType.external)
                throw new Exception(string.Format("Invalid application type: external. listening to message from external applications is not possible"));
            EventType = eventType;
            SourceApplication = sourceApplication;
            Durable = durable;
            AutoDelete = autoDelete;
            Exclusive = exclusive;
            Arguments = arguments ?? new Dictionary<string, object>();
        }
    }
}
