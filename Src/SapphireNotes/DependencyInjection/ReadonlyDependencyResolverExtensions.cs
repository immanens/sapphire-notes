using System;
using Splat;

namespace SapphireNotes.DependencyInjection;

public static class ReadonlyDependencyResolverExtensions
{
    /// <summary>
    /// Получение необходимого сервиса для конфигурации DependencyResolver.
    /// </summary>
    /// <param name="resolver"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<TService>();
        if (service is null)
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(TService)}");
        }

        return service;
    }
}
