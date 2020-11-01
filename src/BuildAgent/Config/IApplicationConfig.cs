namespace BuildAgent.Config
{
    using Newtonsoft.Json;

    using Shared.Converters;

    using System;

    internal interface IApplicationConfig
    {
        /// <summary>
        /// Размер занимаемой приложением памяти, после которого его процесс будет убит
        /// </summary>
        public long MaximumMemoryUsage { get; }

        /// <summary>
        /// Время выполнения приложения, после которого его процесс булет убит
        /// </summary>
        public TimeSpan MaximumExecutionTime { get; }

        /// <summary>
        /// Расширение файла, с которым будет сохранен исходник приложения
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// Папка, в которой будет выполяться сборка
        /// </summary>
        public string BuildDir { get; }

        /// <summary>
        /// Папка, в которй будут выполняться скомпилированные программы
        /// </summary>
        public string RunDir { get; }

        /// <summary>
        /// Шаблон команды сборки
        /// </summary>
        /// <remarks>
        /// Может содержать аргументы:
        /// 0 - имя файла для компиляции
        /// 1 - Имя приложения, которое получится после компиляции
        /// </remarks>
        public string BuildArgsTemplate { get; }

        /// <summary>
        /// Имя приложения компилятора
        /// </summary>
        public string CompillerAppName { get; set; }

        /// <summary>
        /// Является ли язык интерпретируемым
        /// </summary>
        public bool IsInterpreter { get; set; }
    }
}
