using SapphireNotes.Contracts;
using SapphireNotes.Repositories;
using SapphireNotes.Services;
using SapphireNotes.ViewModels;
using Splat;

namespace SapphireNotes.DependencyInjection;

public static class Bootstrapper
{
    /// <summary>
    /// Регистрация сервисов и моделей в DependencyInjection контейнере.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="resolver"></param>
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterServices(services, resolver);
        RegisterViewModels(services, resolver);
    }

    /// <summary>
    /// Регистрация ленивой загрузки для сервисов и репозитория.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="resolver"></param>
    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IPreferencesService>(() => new PreferencesService());
        services.RegisterLazySingleton<INotesMetadataService>(() => new NotesMetadataService());

        services.RegisterLazySingleton<INotesRepository>(() => new FileSystemRepository(
            resolver.GetService<IPreferencesService>()
        ));

        services.RegisterLazySingleton<INotesService>(() => new NotesService(
            resolver.GetService<INotesMetadataService>(),
            resolver.GetService<INotesRepository>()));
    }

    /// <summary>
    /// Регистрация вью-моделей.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="resolver"></param>
    private static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.Register(() => new MainWindowViewModel(
           resolver.GetService<IPreferencesService>(),
           resolver.GetService<INotesService>()
       ));
    }
}
