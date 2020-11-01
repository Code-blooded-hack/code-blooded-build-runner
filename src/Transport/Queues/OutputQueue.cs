namespace Transport.Queues
{
    using Newtonsoft.Json;

    using Shared.Converters;

    using System.Text;

    /// <summary>
    /// Очередь, в которую отправляются запросы из текущей системы
    /// </summary>
    public class OutputQueue : BaseQueue, IOutputQueue
    {
        /// <summary>
        /// Отправляет сообщение в очередь
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        public virtual void Send(string message, string routingKey = null)
        {
            this.DeclareQueue();

            // Лочка нужна, чтобы не было ошибки при отправках сообщений из нескольких потоков одновременно
            lock (this.Connection)
            {
                if (string.IsNullOrEmpty(routingKey))
                {
                    routingKey = string.IsNullOrEmpty(this.InitialConfig.RoutingKey)
                        ? this.InitialConfig.Name
                        : this.InitialConfig.RoutingKey;
                }

                this.Connection.Channel.BasicPublish(
                    this.InitialConfig.ExchangeName ?? string.Empty,
                    this.InitialConfig.Name,
                    true,
                    null,
                    Encoding.UTF8.GetBytes(message));
            }
        }

        public void Send<T>(T message, string routingKey = null)
        {
            var textMessage = JsonConvert.SerializeObject(message, Formatting.None);
            this.Send(textMessage, routingKey);
        }
    }
}