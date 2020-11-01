namespace RequestBuilder
{
    using BuildAgent.Config;
    using BuildAgent.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using Shared;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Transport;
    using Transport.Impl;

    class Program
    {
        static void Main(string[] args)
        {
            var config = ApplicationConfig.CreateWithEnv("config.json");
            if (config == null)
            {
                var example = ApplicationConfig.CreateExample();
                var configText = JsonConvert.SerializeObject(example, Formatting.Indented);

                File.WriteAllText("config-examle.json", configText);

                var standardError = new StreamWriter(Console.OpenStandardError());
                standardError.AutoFlush = true;
                Console.SetError(standardError);
                Console.Error.WriteLine("Error reading config.json. File not found");

                return;
            }

            var services = RegisterServices(config);
            var outputQueue = services.GetRequiredService<IOutputQueue>();
            //var inputQueue = services.GetRequiredService<IInputQueue>();
            //inputQueue.Received = HandleTask;

            var i = 0;
            while (true)
            {
                string path;
                while (true)
                {
                    Console.Write("Please write src file path: ");
                    path = Console.ReadLine();
                    if (File.Exists(path))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid path!");
                    }
                }

                var srcContent = File.ReadAllText(path);
                var data = Base64Encode(srcContent);

                var request = new Request
                {
                    Id = i++,
                    Answer = data,
                    Input = new List<string>
                    {
                        "1",
                        "2",
                        "3"
                    },
                    TimeLimit = TimeSpan.FromSeconds(1),
                    MemoryLimit = 12
                };

                outputQueue.Send(request);
                Console.WriteLine("Sended!");
            }
        }

        private static IServiceProvider RegisterServices(ApplicationConfig config)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IApplicationConfig>(config);
            services.AddSingleton<IRabbitMqConnectionFactory, DefaultRabbitMqConnectionFactory>();
            services.AddSingletonWithFactory<IRabbitMqConnectionFactory, IRabbitMqConnection>(factory => factory.Connect(config.RabbitMqConfig.Connection));
            //services.AddSingletonWithFactory<IRabbitMqConnection, IInputQueue>(factory => factory.GetOrAddInputQueue(config.RabbitMqConfig.InputQueue));
            services.AddSingletonWithFactory<IRabbitMqConnection, IOutputQueue>(factory => factory.GetOrAddOutputQueue(config.RabbitMqConfig.OutputQueue));

            return services.BuildServiceProvider(true);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static async ValueTask<(MessageActionType action, bool requeuee)> HandleTask(IInputQueue sender, string routingKey, string message)
        {
            //Console.WriteLine(message);
            return (MessageActionType.Ack, false);
        }
    }
}
