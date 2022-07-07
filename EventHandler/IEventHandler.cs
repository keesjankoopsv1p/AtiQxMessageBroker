using AtiQxMessageBroker.Messages;

namespace AtiQxMessageBroker.EventHandler
{
    /// <summary>
    /// Event handler interface, this interface contains the necessary functions for handling all shared messages through the messagebroker
    /// </summary>
    public interface IEventHandler
    {
        public bool Handle(IAtiQxMessage message);
    }
}
