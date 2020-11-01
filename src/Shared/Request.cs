namespace Shared
{
    using Newtonsoft.Json;

    using Shared.Converters;

    using System;
    using System.Collections.Generic;

    public class Request
    {
        /// <summary>
        /// Идентификатор задания
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Исходный код на компиляцию
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Лимит времени выполнения
        /// </summary>
        public TimeSpan TimeLimit { get; set; }

        /// <summary>
        /// Лимит занимаемой памяти
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Входные значения
        /// </summary>
        public List<string> Input { get; set; }
    }
}
