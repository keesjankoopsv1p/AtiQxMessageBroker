using AtiQxMessageBroker.Enums;

namespace AtiQxMessageBroker.MessageBrokerClient
{
    /// <summary>
    /// Contains all the information necessary for the MessageBrokerClient to create a connection, build the topology and publish/consume message.
    /// The constructor expects a companyId when the application type is not a multi-tenant application (Guest or AtiQx.ID)
    /// </summary>
    public class MessageBrokerClientConfig
    {
        /// <summary>
        /// Name of your own application, will be ignored if the application property is not 'external'
        /// </summary>
        private string? _externalApplicationName;
        /// <summary>
        /// RabbitMQ username
        /// </summary>
        public readonly string Username;
        /// <summary>
        /// RabbitMQ user password
        /// </summary>
        public readonly string Password;
        /// <summary>
        /// RabbitMQ Server Adress
        /// </summary>
        public readonly string Host;
        /// <summary>
        /// ApplicationType, one of the AtiQx applications or 'external' if used by a third party
        /// </summary>
        public readonly ApplicationType Application;
        /// <summary>
        /// unique CompanyId received from the organisation AtiQx. Will be ignored if the application is a multi-tenant application (guest/atiqx.id)
        /// </summary>
        public readonly string CompanyId;

        public string ExternalApplicationName
        {
            get
            {
                return _externalApplicationName;
            }
            set
            {
                //external application name is not allowed to be the same as one of AtiQx's own applications
                ApplicationType[] applicationTypes = (ApplicationType[])Enum.GetValues(typeof(ApplicationType));
                foreach (ApplicationType type in applicationTypes)
                {
                    if (type.ToString() == value && type != ApplicationType.external)
                        throw new Exception("external application name can't be the same as a name in the ApplicationType Enum");
                }
                _externalApplicationName = value;
            }
        }

        public MessageBrokerClientConfig(string username, string password, string host, ApplicationType application, string companyId = "", string externalApplicationName = "external")
        {
            if (!(Application == ApplicationType.guest || application == ApplicationType.atiqxid) && string.IsNullOrEmpty(companyId))
                throw new Exception(string.Format("Application of type {0} is not a multi-tenant application, so a companyId is required", application.ToString()));
            Username = username;
            Password = password;
            Host = host;
            Application = application;
            CompanyId = companyId;
            ExternalApplicationName = externalApplicationName;
        }
    }
}
