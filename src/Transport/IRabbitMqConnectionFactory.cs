namespace Transport
{
    using System;

    using Transport.Config;

    /// <summary>
    /// Фабрика подключений к RabbitMQ
    /// </summary>
    public interface IRabbitMqConnectionFactory : IDisposable
    {
        /// <summary>
        /// Добавляет соединение с сервером RabbitMQ и подключается к нему
        /// </summary>
        /// <param name="connectionConfig">Экземпляр, содержащий информацию о сервере подключаемого RabbitMQ</param>
        /// <returns>Экземпляр соединения</returns>
        IRabbitMqConnection Connect(ConnectionConfig connectionConfig);
    }
}