namespace BuildAgent.Config
{
    using Transport.Config;

    /// <summary>
    /// Кнофигурация клюента RabbitMQ
    /// </summary>
    internal class RabbitMqConfig
    {
        /// <summary>
        /// Настройки подключения
        /// </summary>
        public ConnectionConfig Connection { get; set; }

        /// <summary>
        /// Настройка очереди входа
        /// </summary>
        public QueueConfig InputQueue { get; set; }

        /// <summary>
        /// Настройка очереди выхода
        /// </summary>
        public QueueConfig OutputQueue { get; set; }
    }
}
