using AtiQxMessageBroker.Enums;
using AtiQxMessageBroker.EventHandler;
using AtiQxMessageBroker.MessageBrokerClient;
using RabbitMQ.Client;

namespace AtiQxMessageBroker.Topology
{
    internal class TopologyBuilder : ITopologyBuilder
    {
        private readonly string _companyId;
        private readonly MessageConsumer _consumer;
        private readonly IModel _channel;
        private readonly ApplicationType _applicationType;
        private readonly List<QueueSubscriptionObject> subscribedEventTypes;
        private readonly string ExternalName;

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="subscribedEventTypes"></param>
        /// <param name="eventHandler"></param>
        public TopologyBuilder(MessageBrokerClientConfig config, List<QueueSubscriptionObject> subscribedEventTypes, IEventHandler eventHandler)
        {
            _companyId = config.CompanyId;
            ExternalName = config.ExternalApplicationName;
            _applicationType = config.Application;
            this.subscribedEventTypes = subscribedEventTypes;

            var factory = new ConnectionFactory()
            {
                HostName = config.Host,
                UserName = config.Username,
                Password = config.Password,
            };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _consumer = new MessageConsumer(_channel, eventHandler);
        }

        /// <summary>
        /// function for building RabbitMQ topology. the AtiQx multi-tenant applications have their own functions.
        /// </summary>
        public void BuildTopology()
        {
            //if applicationType is atiqxid, build atiqxid specific exchange because it is a multi-tenant application
            if (_applicationType == ApplicationType.atiqxid)
            {
                BuildAtiQxIDTopology();
                return;
            }

            //if applicationType is guest, build guest specific exchange because it is a multi-tenant application
            if (_applicationType == ApplicationType.guest)
            {
                BuildGuestTopology();
                return;
            }

            BuildExchanges();
            BuildQueues();
        }

        /// <summary>
        /// function responsible for building the exchanges.
        /// </summary>
        private void BuildExchanges()
        {
            //if applicationType is external, company exchanges are already build by atiqx applications
            if (_applicationType == ApplicationType.external)
                return;

            //build the main exchange for the company.
            _channel.ExchangeDeclare(_companyId, type: "topic", true);

            //bind the new company exchange to all multi-tenant exchanges of AtiQx.
            BindCompanyToMultiTenant();

            //build application specific exchange for the cuurent application
            string applicationExchangeName = string.Format("{0}.{1}", _companyId, _applicationType);
            _channel.ExchangeDeclare(applicationExchangeName, type: "topic", true);
            _channel.ExchangeBind(applicationExchangeName, _companyId, string.Format("{0}.{1}.*", _companyId, _applicationType));

            //build an exchange for every unique atiqx application the current atiqx application wants to listen to, except for multi-tenant applications
            List<ApplicationType> applicationsToListenTo = subscribedEventTypes.Where(item => item.SourceApplication != ApplicationType.guest && item.SourceApplication != ApplicationType.guest)
                .Select(item => item.SourceApplication).Distinct().ToList();

            foreach (ApplicationType applicationType in applicationsToListenTo)
            {
                applicationExchangeName = string.Format("{0}.{1}", _companyId, applicationType);
                _channel.ExchangeDeclare(applicationExchangeName, type: "topic", true);
                _channel.ExchangeBind(applicationExchangeName, _companyId, string.Format("{0}.{1}.*", _companyId, applicationType));
            }
        }

        /// <summary>
        /// function for building the RabbitMQ resources for AtiQxID.
        /// </summary>
        private void BuildAtiQxIDTopology()
        {
            string queueName = "atiqxid.medewerker";
            string exchangeName = "medewerker";

            _channel.ExchangeDeclare(exchangeName, "topic", true);

            _channel.QueueDeclare(queueName, true, false, false);
            _channel.QueueBind(queueName, exchangeName, "*.*.medewerker");
            _channel.BasicConsume(queueName, false, _consumer);
        }

        /// <summary>
        /// function for building the RabbitMQ resources for Guest.
        /// </summary>
        private void BuildGuestTopology()
        {
            string queueName = "guest.medewerker";
            string guestExchangeName = "guest";
            string medewerkerExchangeName = "medewerker";

            _channel.ExchangeDeclare(medewerkerExchangeName, "topic", true);
            _channel.ExchangeDeclare(guestExchangeName, "topic", true);

            _channel.QueueDeclare(queueName, true, false, false);
            _channel.QueueBind(queueName, medewerkerExchangeName, "*.*.medewerker");
            _channel.BasicConsume(queueName, false, _consumer);
        }

        private void BindCompanyToMultiTenant()
        {
            string routingkey = string.Format("{0}.*.", _companyId);
            _channel.ExchangeBind("medewerker", _companyId, routingkey + "medewerker");
        }

        /// <summary>
        /// Builds the queue according to the eventType and application to listen to.
        /// </summary>
        private void BuildQueues()
        {
            foreach (QueueSubscriptionObject subscriptionObject in subscribedEventTypes)
            {
                string queueName;
                string exchangeName;

                //if the current application is an external application, queuename will be like "*companyID*.*externalAppName*.*eventType*
                //the queue will be bound to the main exchange
                if (_applicationType == ApplicationType.external)
                {
                    queueName = string.Format("{0}.{1}.{2}", _companyId, ExternalName, subscriptionObject.EventType);
                    if (subscriptionObject.SourceApplication == ApplicationType.guest)
                        exchangeName = "guest";
                    else
                        exchangeName = _companyId;
                }
                //if the current application is an AtiQx application, queuename will be like *companyID*.*AtiQxAppName*.*eventType*
                //the queue will be bound to the application specific exchange
                else
                {
                    queueName = string.Format("{0}.{1}.{2}", _companyId, _applicationType, subscriptionObject.EventType);
                    if (subscriptionObject.SourceApplication == ApplicationType.guest)
                        exchangeName = "guest";
                    else
                        exchangeName = string.Format("{0}.{1}", _companyId, subscriptionObject.SourceApplication);
                }

                //declare queue
                _channel.QueueDeclare(queueName, subscriptionObject.Durable, subscriptionObject.Exclusive, subscriptionObject.AutoDelete, subscriptionObject.Arguments);

                //bindings
                string routingKey = string.Format("{0}.{1}.{2}", _companyId, subscriptionObject.SourceApplication, subscriptionObject.EventType);
                _channel.QueueBind(queueName, exchangeName, routingKey);

                //receiver
                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
            }
        }
    }
}
