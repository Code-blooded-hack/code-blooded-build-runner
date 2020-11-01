namespace Transport
{
    using System;
    using System.Collections.Generic;

    using RabbitMQ.Client;

    using Transport.Config;

    /// <summary>
    /// Интерфейс соединения с RabbitMQ
    /// </summary>
    public interface IRabbitMqConnection : IDisposable
    {
        /// <summary>
        /// Конфигурация подключения к RabbitMQ, извлекаемая из настроек
        /// </summary>
        ConnectionConfig Config { get; }

        /// <summary>
        /// Канал связи, через который проходит подключение, отправка и приёмка сообщений
        /// </summary>
        IModel Channel { get; }

        /// <summary>
        /// Выполнено ли подключение
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Словарь очередей, которые будут принимать сообщения
        /// </summary>
        IReadOnlyDictionary<string, IInputQueue> InputQueues { get; }

        /// <summary>
        /// Словарь очередей, через которые можно отправлять сообщения
        /// </summary>
        IReadOnlyDictionary<string, IOutputQueue> OutputQueues { get; }

        /// <summary>
        /// Добавляет соединение с сервером RabbitMQ и подключается к нему
        /// </summary>
        /// <param name="connectionConfig">Экземпляр, содержащий информацию о сервере подключаемого RabbitMQ</param>
        void Connect(ConnectionConfig connectionConfig);

        /// <summary>
        /// Добавляет очередь для приема сообщений
        /// </summary>
        /// <param name="queueConfig">Параметры создания очереди</param>
        /// <returns>Созданная или уже существующая очередь</returns>
        IInputQueue GetOrAddInputQueue(QueueConfig queueConfig);

        /// <summary>
        /// Добавляет очередь для отправки сообщений
        /// </summary>
        /// <param name="queueConfig">Параметры создания очереди</param>
        /// <returns>Созданная или уже существующая очередь</returns>
        IOutputQueue GetOrAddOutputQueue(QueueConfig queueConfig);
    }
}