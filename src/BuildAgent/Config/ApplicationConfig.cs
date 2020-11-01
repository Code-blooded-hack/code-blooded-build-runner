namespace BuildAgent.Config
{
    using Newtonsoft.Json;

    using Shared.Converters;

    using System;
    using System.Collections.Generic;
    using System.IO;

    using Transport;
    using Transport.Config;

    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    internal class ApplicationConfig : IApplicationConfig
    {
        private const string MaxMemEnvKey = "AGENT_MEMORY_LIMIT";
        private const string MaxExecutionTimeEnvKey = "AGENT_EXECUTION_TIME_LIMIT";
        private const string ConnectionStringEnvKey = "AGENT_RABBITMQ_CONNECTION_STRING";
        private const string BuildDirEnvKey = "AGENT_BUILD_DIR";
        private const string RunDirEnvKey = "AGENT_RUN_DIR";

        /// <summary>
        /// Размер занимаемой приложением памяти, после которого его процесс будет убит
        /// </summary>
        public long MaximumMemoryUsage { get; set; }

        /// <summary>
        /// Время выполнения приложения, после которого его процесс булет убит
        /// </summary>
        public TimeSpan MaximumExecutionTime { get; set; }

        /// <summary>
        /// Расширение файла, с которым будет сохранен исходник приложения
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Конфигурация клиента RabbitMQ
        /// </summary>
        public RabbitMqConfig RabbitMqConfig { get; set; }

        /// <summary>
        /// Папка, в которой будет выполяться сборка
        /// </summary>
        public string BuildDir { get; set; }

        /// <summary>
        /// Папка, в которй будут выполняться скомпилированные программы
        /// </summary>
        public string RunDir { get; set; }

        /// <summary>
        /// Шаблон команды сборки
        /// </summary>
        /// <remarks>
        /// Может содержать аргументы:
        /// 0 - имя файла для компиляции
        /// 1 - Имя приложения, которое получится после компиляции
        /// </remarks>
        public string BuildArgsTemplate { get; set; }

        /// <summary>
        /// Имя приложения компилятора
        /// </summary>
        public string CompillerAppName { get; set; }

        /// <summary>
        /// Является ли язык интерпретируемым
        /// </summary>
        public bool IsInterpreter { get; set; }

        /// <summary>
        /// Формирует экземпляр <see cref="ApplicationConfig"/> из конфиг файла
        /// </summary>
        /// <param name="jsonConfigFilePath"></param>
        /// <returns></returns>
        public static ApplicationConfig Create(string jsonConfigFilePath)
        {
            if (!File.Exists(jsonConfigFilePath))
            {
                return null;
            }

            var configString = File.ReadAllText(jsonConfigFilePath);
            return JsonConvert.DeserializeObject<ApplicationConfig>(configString);
        }

        /// <summary>
        /// Формирует экземпляр <see cref="ApplicationConfig"/> из конфиг файла и переменных окружения
        /// </summary>
        /// <param name="jsonConfigFilePath"></param>
        /// <returns></returns>
        public static ApplicationConfig CreateWithEnv(string jsonConfigFilePath)
        {
            var result = Create(jsonConfigFilePath);

            if (result != null)
            {
                result.ApplyEnv(MaxMemEnvKey, (cfg, value) => cfg.MaximumMemoryUsage = int.Parse(value));
                result.ApplyEnv(BuildDirEnvKey, (cfg, value) => cfg.BuildDir = value);
                result.ApplyEnv(RunDirEnvKey, (cfg, value) => cfg.RunDir = value);
                result.ApplyEnv(MaxExecutionTimeEnvKey, (cfg, value) => cfg.MaximumExecutionTime = TimeSpan.Parse(value));
                result.ApplyEnv(ConnectionStringEnvKey, (cfg, value) => cfg.ApplyConnectionString(value));
            }

            return result;
        }

        public static ApplicationConfig CreateExample()
        {
            QueueConfig CreateConfig(string name, string excangeName, string key)
            {
                return new QueueConfig()
                {
                    Name = name,
                    RoutingKey = key,
                    Durable = true,
                    Exclusive = false,
                    AutoDelete = false,
                    ExchangeName = excangeName,
                    ExchangeType = "direct",
                    DefaultAction = MessageActionType.Ack
                };
            }

            var parentDir = Path.GetDirectoryName(Environment.CurrentDirectory);

            return new ApplicationConfig()
            {
                RabbitMqConfig = new RabbitMqConfig()
                {
                    Connection = new ConnectionConfig("login", "password", "localhost", 15672, null, 1),
                    InputQueue = CreateConfig("input_queue", "main_exchange", "cpp.reference"),
                    OutputQueue = CreateConfig("output_queue", "main_exchange", "output_queue")
                },
                FileExtension = ".cpp",
                MaximumMemoryUsage = 1024,
                MaximumExecutionTime = TimeSpan.FromMinutes(1),
                BuildDir = Path.Combine(parentDir, "Build"),
                RunDir = Path.Combine(parentDir, "Run"),
                BuildArgsTemplate = "{0} -o {1}",
                CompillerAppName = "gcc"
            };
        }

        private void ApplyEnv(string key, Action<ApplicationConfig, string> appyer)
        {
            var env = Environment.GetEnvironmentVariable(key);

            if (!string.IsNullOrEmpty(env))
            {
                appyer(this, env);
            }
        }

        private void ApplyConnectionString(string connectionString)
        {
            var old = this.RabbitMqConfig.Connection;
            var connectionParamsArray = connectionString.Split(';');
            var connectionParams = new Dictionary<string, string>();
            foreach (var connectionParam in connectionParamsArray)
            {
                var pair = connectionParam.Split('=');
                connectionParams.Add(pair[0].Trim().ToLower(), pair[1].Trim());
            }

            if (!connectionParams.TryGetValue("login", out var login))
            {
                login = old.Login;
            }

            if (!connectionParams.TryGetValue("password", out var password))
            {
                password = old.Password;
            }

            if (!connectionParams.TryGetValue("host", out var host))
            {
                host = old.Host;
            }

            var port = old.Port;
            if (connectionParams.TryGetValue("port", out var portString))
            {
                port = int.Parse(portString);
            }

            if (!connectionParams.TryGetValue("virtualhost", out var virtualhost))
            {
                virtualhost = old.VirtualHost;
            }

            var prefetchSize = old.PrefetchSize;
            if (connectionParams.TryGetValue("port", out var prefetchSizeString))
            {
                prefetchSize = ushort.Parse(prefetchSizeString);
            }

            this.RabbitMqConfig.Connection = new ConnectionConfig(login, password, host, port, virtualhost, prefetchSize);
        }
    }
}
