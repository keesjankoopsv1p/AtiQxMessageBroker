using AtiQxMessageBroker.Messages;

namespace AtiQxMessageBroker.EventPublisher
{
    internal interface IAtiQxEventPublisher
    {
        public void Publish(IAtiQxMessage message);
    }
}
