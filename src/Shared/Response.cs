namespace Shared
{
    using Newtonsoft.Json;

    using Shared.Converters;

    using System;
    using System.Collections.Generic;

    public class Response
    {
        public Response()
        {
            this.ExecutionTime = TimeSpan.FromSeconds(0);
            this.MemoryUsage = 0;
            this.Tests = new List<TestResult>();
        }

        public Response(int id) : this()
        {
            this.Id = id;
        }

        /// <summary>
        /// Идентификатор задания
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Время выполнения
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Использованная память
        /// </summary>
        public long MemoryUsage { get; set; }

        // <summary>
        /// Результаты тестов
        /// </summary>
        public List<TestResult> Tests { get; set; }
    }
}
