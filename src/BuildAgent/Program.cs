namespace BuildAgent
{
    using BuildAgent.Config;
    using BuildAgent.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using System;
    using System.IO;

    using Transport;
    using Transport.Impl;

    class Program
    {
        static int Main(string[] args)
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

                return -1;
            }

            //BuildDemoConfig();
            return RegisterServices(config)
                .DoFunc<Application, int>(x => x.Start());
        }

        private static IServiceProvider RegisterServices(ApplicationConfig config)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IApplicationConfig>(config);
            services.AddSingleton<IRabbitMqConnectionFactory, DefaultRabbitMqConnectionFactory>();
            services.AddSingletonWithFactory<IRabbitMqConnectionFactory, IRabbitMqConnection>(factory => factory.Connect(config.RabbitMqConfig.Connection));
            services.AddSingletonWithFactory<IRabbitMqConnection, IInputQueue>(factory => factory.GetOrAddInputQueue(config.RabbitMqConfig.InputQueue));
            services.AddSingletonWithFactory<IRabbitMqConnection, IOutputQueue>(factory => factory.GetOrAddOutputQueue(config.RabbitMqConfig.OutputQueue));
            services.AddSingleton<Application>();

            return services.BuildServiceProvider(true);
        }
    }
}
