namespace BuildAgent.Extensions
{
    using BuildAgent.Config;

    using Microsoft.Extensions.DependencyInjection;

    using System;

    internal static class IoCExtensions
    {
        public static void DoAction<TService>(this IServiceProvider services, Action<TService> action)
        {
            action(services.GetRequiredService<TService>());
        }

        public static TResult DoFunc<TService, TResult>(this IServiceProvider services, Func<TService, TResult> action)
        {
            return action(services.GetRequiredService<TService>());
        }

        public static void AddSingletonWithFactory<TFactory, TService>(this IServiceCollection services, Func<TFactory, TService> factory)
            where TFactory : class
            where TService : class
        {
            services.AddSingleton(serviceProvider => {
                var innerFasctory = serviceProvider.GetRequiredService<TFactory>();
                return factory(innerFasctory);
            });
        }
    }
}
