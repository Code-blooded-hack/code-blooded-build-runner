namespace Transport.Impl
{
    using System.Collections.Concurrent;

    using Transport.Config;

    /// <summary>
    /// Фабрика подключений к RabbitMQ
    /// </summary>
    public class DefaultRabbitMqConnectionFactory
        : IRabbitMqConnectionFactory
    {
        /// <summary>
        /// Словарь подключений
        /// </summary>
        private readonly ConcurrentDictionary<ConnectionConfig, IRabbitMqConnection> connections 
            = new ConcurrentDictionary<ConnectionConfig, IRabbitMqConnection>();

        /// <summary>
        /// Выполняет подключение к RabbitMQ, если оно еще не выполнено, возвращает экземпляр созданного подключения
        /// </summary>
        /// <param name="connectionConfig">Параметры подключения</param>
        /// <returns>Экземпляр подключения</returns>
        public IRabbitMqConnection Connect(ConnectionConfig connectionConfig)
        {
            return this.connections.GetOrAdd(
                connectionConfig, 
                x =>
                {
                    var newInstance = new RabbitMqConnection();
                    newInstance.Connect(x);
                    return newInstance;
                });
        }

        /// <summary>
        /// Выполняет освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            foreach (var connection in this.connections.Values)
            {
                connection.Dispose();
            }
        }
    }
}