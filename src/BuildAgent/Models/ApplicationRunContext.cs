namespace BuildAgent.Models
{
    using System;

    class ApplicationRunContext
    {
        /// <summary>
        /// Максимальный размер занимаемой памяти, после которого тест считается проваленым
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Максимальное время выполнения, после которого тест считается проваленым
        /// </summary>
        public TimeSpan ExecutionTimeLimit { get; set; }

        /// <summary>
        /// Имя запускаемого приложения\скрипта
        /// </summary>
        public string AppName { get; set; }

        public bool IsInterpreter { get; set; }
    }
}
