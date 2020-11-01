namespace Transport.Config
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using System.ComponentModel.DataAnnotations;

    public class QueueConfig
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Ключ
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// Очередь остается после перезапуска сервера RabbitMQ
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Очередь может использоваться только соединением, в котором она была создана
        /// </summary>
        public bool Exclusive { get; set; } = false;

        /// <summary>
        /// Очередь автоматически удаляется при неиспользовании
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// Наименование узла обмена, куда поставляются сообщения
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Тип узла обмена
        /// </summary>
        public string ExchangeType { get; set; } = "direct";

        /// <summary>
        /// Действие по умолчанию
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageActionType DefaultAction { get; set; }
    }
}
