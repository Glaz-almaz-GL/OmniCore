using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Hybrid.Helpers;
using OmniCore.Hybrid.Interfaces;
using OmniCore.Hybrid.Services;

namespace OmniCore.Hybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();

            ConfigureMauiApp(builder);
            ConfigureServices(builder);

            OSPlatforms currentOS = OSDetector.DetectCurrentOS();
            List<IModule> resultModules = [];

            // Создаем временный провайдер для загрузки модулей
            using (ServiceProvider tempProvider = builder.Services.BuildServiceProvider())
            {

                // Создаем загрузчик модулей
                ModuleLoader moduleLoader = new(tempProvider.GetService<ILogger<ModuleLoader>>());

                // Загружаем модули
                //string modulesDirectory = Path.Combine(AppContext.BaseDirectory, "Modules");
                string modulesDirectory = AppContext.BaseDirectory;

                try
                {
                    if (Directory.Exists(modulesDirectory))
                    {
                        //foreach (string moduleDir in Directory.GetDirectories(modulesDirectory, "*", SearchOption.TopDirectoryOnly))
                        //{
                        string moduleDir = AppContext.BaseDirectory;

                        IReadOnlyCollection <IModule> modules = moduleLoader.LoadModules(
                        moduleDir,
                        currentOS,
                        tempProvider);

                        // Регистрируем модули и их сервисы
                        foreach (IModule module in modules)
                        {
                            RegisterModule(builder.Services, module);
                            resultModules.Add(module);
                        }
                        //}
                    }
                }
                catch (Exception ex)
                {
                    ILogger<ModuleLoader>? logger = tempProvider.GetService<ILogger<ModuleLoader>>();
                    logger?.LogCritical(ex, "Failed to load modules");
                }
            }

            MauiApp finalApp = builder.Build();

            using (IServiceScope scope = finalApp.Services.CreateScope())
            {
                foreach (IModule module in resultModules)
                {
                    try
                    {
                        module.Initialize(scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        ILogger<MauiApp>? logger = scope.ServiceProvider.GetService<ILogger<MauiApp>>();
                        logger?.LogError(ex, "Failed to initialize module {ModuleName}", module.Title);
                    }
                }

                // Передаем загруженные модули в ModuleManager для управления состоянием
                IModuleManager moduleManager = scope.ServiceProvider.GetRequiredService<IModuleManager>();
                moduleManager.RegisterModules(resultModules);
            }

            return finalApp;
        }

        private static void ConfigureMauiApp(MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));
        }

        private static void ConfigureServices(MauiAppBuilder builder)
        {
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddLocalization();

            builder.Services.AddLogging();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddMudServices();
            builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
            builder.Services.AddSingleton<ILayoutStateService, LayoutStateService>();
            builder.Services.AddSingleton<IModuleManager, ModuleManager>();
        }

        private static void RegisterModule(IServiceCollection services, IModule module)
        {
            services.AddSingleton<IModule>(module);

            if (module is IModuleSettingsProvider settingsProvider)
            {
                services.AddSingleton<IModuleSettingsProvider>(settingsProvider);
            }

            module.AddModuleServices(services);
        }
    }
}