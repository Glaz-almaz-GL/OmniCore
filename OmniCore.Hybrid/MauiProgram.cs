using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Hybrid.Interfaces;
using OmniCore.Hybrid.Services;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace OmniCore.Hybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder builder = MauiApp.CreateBuilder();

            // 1. Базовая настройка приложения и сервисов
            ConfigureMauiApp(builder);
            ConfigureServices(builder);

            // 2. Определение окружения
            OSPlatforms currentOS = DetectCurrentOS();

            // 3. Регистрация модулей (включает динамическую загрузку сборок)
            ServiceProvider tempProvider = builder.Services.BuildServiceProvider();
            IAppSettingsService? globalSettings = tempProvider.GetService<IAppSettingsService>();

            // Передаем путь к директории для сканирования
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            RegisterModules(builder, tempProvider, globalSettings, currentOS, appDirectory);

            // 4. Настройки для режима отладки
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
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
            builder.Services.AddMudServices();
            builder.Services.AddSingleton<IAppSettingsService>(new AppSettingsService());
        }

        private static OSPlatforms DetectCurrentOS()
        {
            OSPlatforms currentOS = OSPlatforms.None;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentOS |= OSPlatforms.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                currentOS |= OSPlatforms.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                currentOS |= OSPlatforms.MacOS;
            }

            return currentOS;
        }

        private static void RegisterModules(MauiAppBuilder builder, ServiceProvider provider, IAppSettingsService? settings, OSPlatforms currentOS, string appDirectory)
        {
            // Загружаем сборки из файловой системы, а не берем из уже загруженных
            IEnumerable<Assembly> assemblies = LoadAssembliesFromDirectory(appDirectory);

            // Ищем модули во всех загруженных сборках
            List<Type> moduleTypes = [.. assemblies
                .SelectMany(GetTypesFromAssembly)
                .Where(IsValidModule)];

            Debug.WriteLine($"[Module] Всего найдено типов модулей: {moduleTypes.Count}");

            int registeredCount = 0;
            foreach (Type? type in moduleTypes)
            {
                if (!TryCreateModuleInstance(provider, type, out IModule? moduleInstance))
                {
                    continue;
                }

                if (!IsModuleSupported(moduleInstance, currentOS))
                {
                    continue;
                }

                if (!IsModuleEnabled(moduleInstance, settings))
                {
                    continue;
                }

                RegisterModule(builder, moduleInstance);
                registeredCount++;
            }

            Debug.WriteLine($"[Module] ИТОГО зарегистрировано модулей: {registeredCount}");
        }

        private static List<Assembly> LoadAssembliesFromDirectory(string directoryPath)
        {
            List<Assembly> loadedAssemblies = [];

            try
            {
                string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll");
                Debug.WriteLine($"[Loader] Найдено DLL файлов: {dllFiles.Length}");

                foreach (string dll in dllFiles)
                {
                    try
                    {
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                        loadedAssemblies.Add(assembly);
                    }
                    catch (BadImageFormatException)
                    {
                        // Игнорируем нативные библиотеки или библиотеки другой архитектуры
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Loader] Ошибка загрузки сборки {Path.GetFileName(dll)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Loader] Ошибка доступа к директории {directoryPath}: {ex.Message}");
            }

            return loadedAssemblies;
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Возвращаем только успешно загруженные типы
                return ex.Types.Where(t => t != null)!;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Module] Ошибка GetTypes() для {assembly.FullName}: {ex.Message}");
                return [];
            }
        }

        private static bool IsValidModule(Type type)
        {
            bool isModule = typeof(IModule).IsAssignableFrom(type);
            bool isValid = !type.IsInterface && !type.IsAbstract;

            if (isModule && isValid)
            {
                Debug.WriteLine($"[Module] Найден тип модуля: {type.FullName} в сборке {type.Assembly.GetName().Name}");
            }

            return isModule && isValid;
        }

        private static bool TryCreateModuleInstance(ServiceProvider provider, Type type, out IModule module)
        {
            module = null!;
            try
            {
                module = (IModule)ActivatorUtilities.CreateInstance(provider, type);
                Debug.WriteLine($"[Module] Экземпляр создан: {type.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Module] ОШИБКА создания экземпляра {type.Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private static bool IsModuleSupported(IModule module, OSPlatforms currentOS)
        {
            bool supported = (module.SupportedOS & currentOS) != OSPlatforms.None;
            if (!supported)
            {
                Debug.WriteLine($"[Module] Пропущен: {module.GetType().Name} (Неподдерживаемая ОС: {module.SupportedOS}, текущая: {currentOS})");
            }
            return supported;
        }

        private static bool IsModuleEnabled(IModule module, IAppSettingsService? settings)
        {
            if (settings == null)
            {
                return false;
            }

            bool enabled = settings.IsRouteEnabled(module.BaseRoute);
            if (!enabled)
            {
                Debug.WriteLine($"[Module] Пропущен: {module.GetType().Name} (Отключен в настройках, Route: {module.BaseRoute})");
            }
            return enabled;
        }

        private static void RegisterModule(MauiAppBuilder builder, IModule module)
        {
            builder.Services.AddSingleton<IModule>(module);

            // Регистрируем как IModuleSettingsProvider, если модуль его реализует
            if (module is IModuleSettingsProvider settingsProvider)
            {
                builder.Services.AddSingleton<IModuleSettingsProvider>(settingsProvider);
                Debug.WriteLine($"[Module] Зарегистрирован провайдер настроек: {module.GetType().Name}");
            }

            // Вызываем метод модуля для регистрации его внутренних сервисов
            module.AddModuleServices(builder.Services);

            Debug.WriteLine($"[Module] Успешно зарегистрирован: {module.GetType().Name} (Route: {module.BaseRoute})");
        }
    }
}