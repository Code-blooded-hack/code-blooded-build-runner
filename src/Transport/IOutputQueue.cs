namespace Transport
{
    /// <summary>
    /// Интерфейс очереди отправки
    /// </summary>
    public interface IOutputQueue : IBaseQueue
    {
        /// <summary>
        /// Отправляет сообщение в очередь
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        void Send(string message, string routingKey = null);

        /// <summary>
        /// Отправляет сообщение в очередь
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        void Send<T>(T message, string routingKey = null);
    }
}