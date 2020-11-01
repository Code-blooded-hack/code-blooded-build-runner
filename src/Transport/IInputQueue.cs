namespace Transport
{
    using System.Threading.Tasks;

    /// <summary>
    /// Делегат события, возникающего при получении сообщения из входящей очереди
    /// </summary>
    /// <param name="sender">Источник сообщения</param>
    /// <param name="message">Полученное сообщение</param>
    /// <returns>true, если сообщение обработано успешно, false, если сообщение нужно вернуть в очередь</returns>
    public delegate ValueTask<(MessageActionType action, bool requeuee)> EventGetSourceDataHandler(IInputQueue sender, string routingKey, string message);

    /// <summary>
    /// Интерфейс очереди получения сообщений
    /// </summary>
    public interface IInputQueue : IBaseQueue
    {
        /// <summary>
        /// Событие приема сообщения
        /// </summary>
        EventGetSourceDataHandler Received { get; set; }
    }
}