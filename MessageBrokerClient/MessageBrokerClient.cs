using AtiQxMessageBroker.EventHandler;
using AtiQxMessageBroker.EventPublisher;
using AtiQxMessageBroker.Messages;
using AtiQxMessageBroker.Topology;

namespace AtiQxMessageBroker.MessageBrokerClient
{
    /// <summary>
    /// core class of the AtiQxMessageBroker library.<br></br>
    /// All functionality should be used through this class.<br></br>
    /// To instantiate, pass a messagebrokerConfig, and your own implementation of the IEventhandler interface responsible for handling all the messages.
    /// </summary>
    public sealed class MessageBrokerClient
    {
        private static readonly MessageBrokerClient instance = new();
        public MessageBrokerClientConfig _config;
        private ITopologyBuilder _topologyBuilder;
        private IAtiQxEventPublisher _eventPublisher;
        public bool IsConnected { get; private set; }

        static MessageBrokerClient()
        {
        }

        private MessageBrokerClient()
        {
        }

        public static MessageBrokerClient Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// set the configuration for the messageBrokerClient
        /// after the configuration is set correctly, all functionality will be available.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="eventTypesToSubscribe"></param>
        /// <param name="eventhandler"></param>
        /// <exception cref="Exception"></exception>
        public void SetConfiguration(MessageBrokerClientConfig config, List<QueueSubscriptionObject> eventTypesToSubscribe, IEventHandler eventhandler)
        {
            //to prevent applications from listening to their own events
            if (eventTypesToSubscribe.Select(item => item.SourceApplication).Contains(config.Application))
                throw new Exception(string.Format("Invalid configuration: Application of type: {0} is not able to listen to it's own events.", config.Application));

            _config = config;
            _topologyBuilder = new TopologyBuilder(config, eventTypesToSubscribe, eventhandler);
            _eventPublisher = new AtiQxEventPublisher(config);
            IsConnected = true;
        }

        /// <summary>
        /// build the topology for the entire application based on application type
        /// </summary>
        public void BuildTopology()
        {
            if (!IsConnected)
                throw new Exception("MessageBrokerClient configuration has not been set yet");
            _topologyBuilder.BuildTopology();
        }

        /// <summary>
        /// Share an AtiQxMessage over the message bus
        /// </summary>
        /// <param name="message"></param>
        public void PublishMessage(IAtiQxMessage message)
        {
            if (!IsConnected)
                throw new Exception("MessageBrokerClient configuration has not been set yet");
            _eventPublisher.Publish(message);
        }
    }
}
