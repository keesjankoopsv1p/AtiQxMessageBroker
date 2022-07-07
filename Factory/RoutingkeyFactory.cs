using AtiQxMessageBroker.Enums;

namespace AtiQxMessageBroker.Factory
{
    /// <summary>
    /// Static class that is responsible for retrieving routing keys corresponding to the given EventType
    /// e.g. EventType 'medewerker' will return routing key 'medewerker'
    /// </summary>
    public static class RoutingkeyFactory
    {
        public static string GetRoutingKey(EventType eventType)
        {
            return eventType switch
            {
                EventType.medewerker => "medewerker",
                EventType.boeking => "boeking",
                EventType.toegangsprofiel => "toegangsprofiel",
                _ => "",
            };
        }
    }
}
