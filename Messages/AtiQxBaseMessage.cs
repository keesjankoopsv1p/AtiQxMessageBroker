using AtiQxMessageBroker.Enums;
using AtiQxMessageBroker.Factory;
using Newtonsoft.Json;
using System.Text;

namespace AtiQxMessageBroker.Messages
{
    /// <summary>
    /// base class for all shared messages that can be published.<br></br>
    /// This class serves as a wrapper for shared data objects.<br></br> 
    /// It enforces shared data objects to contain all necessary metadata that is needed when sending the object through the message bus.
    /// </summary>
    public class AtiQxBaseMessage : IAtiQxMessage
    {
        public Guid EventId { get; }
        public DateTime TimeStamp { get; }
        public Dictionary<string, string> MessageBody { get; set; }
        public bool Persistent { get; set; }
        public string Routingkey { get; }
        public EventType EventType { get; }
        public string Action { get; }
        public ApplicationType SourceApplication { get; }
        public string CompanyId { get; }

        public AtiQxBaseMessage(EventType eventType, string action, Dictionary<string, string>?
            messageBody = null, string? routingkey = null, bool persistent = true, string companyId = "")
        {
            EventId = new Guid();
            TimeStamp = DateTime.Now;
            EventType = eventType;
            Action = action;
            Persistent = persistent;
            MessageBody = messageBody ?? new Dictionary<string, string>();
            Routingkey = routingkey ?? RoutingkeyFactory.GetRoutingKey(eventType);
            CompanyId = companyId;
        }

        internal AtiQxBaseMessage(Guid eventId, DateTime timeStamp, EventType eventType, string action, ApplicationType sourceApplication,
            Dictionary<string, string> messageBody, string routingkey, bool persistent, string companyId)
        {
            EventId = eventId;
            TimeStamp = timeStamp;
            EventType = eventType;
            Action = action;
            SourceApplication = sourceApplication;
            Persistent = persistent;
            MessageBody = messageBody;
            Routingkey = routingkey;
            CompanyId = companyId;
        }

        public Dictionary<string, object> GetHeaders()
        {
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("action", Action.ToString());
            return headers;
        }

        public byte[] GetMessageBody()
        {
            return Encoding.Default.GetBytes(JsonConvert.SerializeObject(MessageBody));
        }
    }
}
