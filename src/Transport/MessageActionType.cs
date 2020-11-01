namespace Transport
{
    public enum MessageActionType
    {
        /// <summary>
        /// Ни чего не делать
        /// </summary>
        None,

        /// <summary>
        /// Отменить сообщение
        /// </summary>
        Reject,

        /// <summary>
        /// Подтвердить получение
        /// </summary>
        Ack,
    }
}