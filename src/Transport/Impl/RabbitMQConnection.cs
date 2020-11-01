namespace Transport.Impl
{
    using System.Collections.Generic;

    using RabbitMQ.Client;

    using Transport.Config;
    using Transport.Queues;

    /// <summary>
    /// соединение с RabbitMQ
    /// </summary>
    public class RabbitMqConnection : IRabbitMqConnection
    {
        /// <summary>
        /// Список добавленных очередей отправки
        /// </summary>
        private readonly Dictionary<string, IOutputQueue> outputQueues = 
            new Dictionary<string, IOutputQueue>();

        /// <summary>
        /// Список добавленных очередей приема
        /// </summary>
        private readonly Dictionary<string, IInputQueue> inputQueues = 
            new Dictionary<string, IInputQueue>();

        /// <summary>
        /// Конфигурация подключения к RabbitMQ, извлекаемая из настроек
        /// </summary>
        public ConnectionConfig Config { get; private set; }

        /// <summary>
        /// Словарь очередей входящих сообщений
        /// </summary>
        public IReadOnlyDictionary<string, IInputQueue> InputQueues
        {
            get { return this.inputQueues; }
        }

        /// <summary>
        /// Словарь очередей исходящих сообщений
        /// </summary>
        public IReadOnlyDictionary<string, IOutputQueue> OutputQueues
        {
            get { return this.outputQueues; }
        }

        /// <summary>
        /// Канал связи, через который проходит подключение, отправка и приёмка сообщений
        /// </summary>
        public IModel Channel { get; private set; }

        /// <summary>
        /// Выполнено ли подключение
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.Channel != null;
            }
        }

        /// <summary>
        /// Выполняет подключение к серверу RabbitMQ
        /// </summary>
        /// <param name="config">Конфигурация подключения</param>
        public void Connect(ConnectionConfig config)
        {
            this.Config = config;

            var rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = this.Config.Host,
                Port = this.Config.Port,
                UserName = this.Config.Login,
                Password = this.Config.Password,
                VirtualHost = this.Config.VirtualHost,
                DispatchConsumersAsync = true
            };

            this.Channel = rabbitMqConnectionFactory.CreateConnection().CreateModel();
            this.Channel.BasicQos(0, this.Config.PrefetchSize, false);
        }

        /// <summary>
        /// Создает очередь для приема сообщений
        /// </summary>
        /// <param name="queueConfig">Параметры создания очереди</param>
        /// <returns>True, если очередь была создана, false если возвращена уже существующая очередь</returns>
        public IInputQueue GetOrAddInputQueue(QueueConfig queueConfig)
        {
            if (!this.InputQueues.TryGetValue(queueConfig.Name, out var inputQueue))
            {
                inputQueue = new InputQueue();
                inputQueue.Init(this, queueConfig);

                this.inputQueues.Add(queueConfig.Name, inputQueue);
            }

            return inputQueue;
        }

        /// <summary>
        /// Создает очередь для отправки сообщений
        /// </summary>
        /// <param name="queueConfig">Параметры создания очереди</param>
        /// <returns>True, если очередь была создана, false если возвращена уже существующая очередь</returns>
        public IOutputQueue GetOrAddOutputQueue(QueueConfig queueConfig)
        {
            if (!this.OutputQueues.TryGetValue(queueConfig.Name, out var outputQueue))
            {
                outputQueue = new OutputQueue();
                outputQueue.Init(this, queueConfig);

                this.outputQueues.Add(queueConfig.Name, outputQueue);
            }

            return outputQueue;
        }

        /// <summary>
        /// Закрывает подключение к RabbitMQ
        /// </summary>
        public void Dispose()
        {
            // TODO Проверить, как работает, возможно ещё что-либо надо сделать
            this.Channel.Close();
        }
    }
}