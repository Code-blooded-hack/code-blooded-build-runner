namespace Transport
{
    using Transport.Config;

    /// <summary>
    /// Базовый интерфейс очередей
    /// </summary>
    public interface IBaseQueue
    {
        /// <summary>
        /// Соединение, которому принадлежит очередь
        /// </summary>
        IRabbitMqConnection Connection { get; }

        /// <summary>
        /// Конфигурация очереди
        /// </summary>
        QueueConfig InitialConfig { get; }

        /// <summary>
        /// Инициализирует очередь
        /// </summary>
        /// <param name="connection">Соединение, которому принадлежит очередь</param>
        /// <param name="initialConfig">Конфигурация очереди</param>
        void Init(IRabbitMqConnection connection, QueueConfig initialConfig);
    }
}