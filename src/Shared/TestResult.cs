namespace Shared
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum TestStatus
    {
        Ok,
        PresentationError,
        TimeLimitError,
        MemoryLimitError,
        RuntimeError,
        CompilationError,
        SecurityViolationError
    }

    public class TestResult
    {
        /// <summary>
        /// Входные значения
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Вывод приложения
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Статус выполнения теста
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TestStatus Status { get; set; }
    }
}
