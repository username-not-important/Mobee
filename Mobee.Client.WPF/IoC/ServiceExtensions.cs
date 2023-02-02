using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mobee.Client.WPF.IoC;

public static class ServiceExtensions
{
    public static void AddAbstractFactory<TAbstract>(this IServiceCollection services)
        where TAbstract : class
    {
        services.AddTransient<TAbstract>();
        services.AddSingleton<Func<TAbstract>>(x => () => x.GetService<TAbstract>()!);
        services.AddSingleton<IAbstractFactory<TAbstract>, AbstractFactory<TAbstract>>();
    }
}