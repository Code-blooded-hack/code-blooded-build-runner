namespace Transport.Queues
{
    using System.Collections.Generic;

    using Transport.Config;

    /// <summary>
    /// Базовый класс для очередей RabbitMQ
    /// </summary>
    public abstract class BaseQueue : IBaseQueue
    {
        /// <summary>
        /// Конфигурация очереди
        /// </summary>
        public QueueConfig InitialConfig { get; private set; }

        /// <summary>
        /// Соединение, которому принадлежит очередь
        /// </summary>
        public IRabbitMqConnection Connection { get; private set; }

        /// <summary>
        /// Выполняет инициализацию очереди
        /// </summary>
        /// <param name="connection">Соединение, которому принадлежит очередь</param>
        /// <param name="initialConfig">Конфигурация очереди</param>
        public virtual void Init(IRabbitMqConnection connection, QueueConfig initialConfig)
        {
            this.Connection = connection;
            this.InitialConfig = initialConfig;
        }

        /// <summary>
        /// Создает очередь
        /// </summary>
        protected virtual void DeclareQueue()
        {
            lock (this.Connection)
            {
                this.Connection.Channel.QueueDeclare(
                    this.InitialConfig.Name,
                    this.InitialConfig.Durable,
                    this.InitialConfig.Exclusive,
                    this.InitialConfig.AutoDelete,
                    null);

                if (!string.IsNullOrEmpty(this.InitialConfig.ExchangeName))
                {
                    var arguments = new Dictionary<string, object>();

                    this.Connection.Channel.ExchangeDeclare(
                        this.InitialConfig.ExchangeName,
                        this.InitialConfig.ExchangeType,
                        this.InitialConfig.Durable,
                        this.InitialConfig.AutoDelete,
                        arguments);

                    var routingKey = string.IsNullOrEmpty(this.InitialConfig.RoutingKey)
                        ? this.InitialConfig.Name
                        : this.InitialConfig.RoutingKey;

                    this.Connection.Channel.QueueBind(
                        this.InitialConfig.Name,
                        this.InitialConfig.ExchangeName,
                        routingKey,
                        arguments);
                }
            }
        }
    }
}