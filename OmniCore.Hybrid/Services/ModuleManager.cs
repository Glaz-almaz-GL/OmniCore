using Microsoft.Extensions.Logging;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Hybrid.Helpers;
using OmniCore.Hybrid.Interfaces;

namespace OmniCore.Hybrid.Services
{
    /// <summary>
    /// Управляет жизненным циклом, состоянием и регистрацией модулей в приложении.
    /// Обеспечивает потокобезопасный доступ к коллекции модулей и реактивное уведомление UI
    /// об изменениях состояния через систему событий.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Класс не поддерживает динамическую выгрузку модулей — для освобождения ресурсов
    /// требуется перезапуск приложения.
    /// </para>
    /// <para>
    /// Все операции с коллекциями модулей потокобезопасны и синхронизированы через <see cref="Lock"/>.
    /// </para>
    /// </remarks>
    internal sealed partial class ModuleManager(IAppSettingsService appSettingsService, ILogger<ModuleManager>? logger = null) : IModuleManager
    {
        /// <summary>
        /// Событие, возникающее при регистрации нового модуля в менеджере.
        /// </summary>
        /// <remarks>
        /// Событие не генерируется при пакетной регистрации через <see cref="RegisterModules(IEnumerable{IModule})"/>.
        /// Вместо этого вызывается событие <see cref="ModulesBulkRegistered"/>.
        /// </remarks>
        public event Action<IModule>? ModuleRegistered;

        /// <summary>
        /// Событие, возникающее при изменении состояния модуля (включение или выключение).
        /// </summary>
        /// <remarks>
        /// Подписчики должны использовать это событие для обновления UI (например, перерисовки сайдбара).
        /// В Blazor-компонентах рекомендуется вызывать <c>InvokeAsync(StateHasChanged)</c> в обработчике.
        /// </remarks>
        public event Action<IModule>? ModuleStateChanged;

        /// <summary>
        /// Событие, возникающее при пакетной регистрации нескольких модулей.
        /// </summary>
        /// <remarks>
        /// Это событие генерируется один раз после регистрации всех модулей,
        /// что позволяет избежать множественных перерисовок UI.
        /// </remarks>
        public event Action<IReadOnlyCollection<IModule>>? ModulesBulkRegistered;

        private bool _disposed;

        /// <summary>
        /// Флаг для подавления генерации одиночных событий при пакетных операциях.
        /// </summary>
        /// <remarks>
        /// Когда <c>true</c>, событие <see cref="ModuleRegistered"/> не вызывается для каждого модуля.
        /// Вместо этого после завершения пакетной операции вызывается <see cref="ModulesBulkRegistered"/>.
        /// </remarks>
        private bool _suppressEvents;

        /// <summary>
        /// Объект синхронизации для обеспечения потокобезопасного доступа к коллекциям модулей.
        /// </summary>
        private readonly Lock _lock = new();

        /// <summary>
        /// Сервис логирования для диагностики операций менеджера модулей.
        /// </summary>
        private readonly ILogger<ModuleManager>? _logger = logger;

        /// <summary>
        /// Сервис настроек приложения для проверки и изменения состояния маршрутов модулей.
        /// </summary>
        private readonly IAppSettingsService _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));

        /// <summary>
        /// Коллекция всех зарегистрированных модулей.
        /// </summary>
        private readonly HashSet<IModule> _registeredModules = [];

        /// <summary>
        /// Коллекция активных модулей (включенных и поддерживаемых текущей ОС).
        /// </summary>
        private readonly HashSet<IModule> _activatedModules = [];

        /// <summary>
        /// Регистрирует одиночный модуль в менеджере.
        /// </summary>
        /// <param name="module">Модуль для регистрации. Не может быть <c>null</c>.</param>
        /// <remarks>
        /// <para>
        /// Если модуль уже зарегистрирован, метод логирует предупреждение и завершает работу без изменений.
        /// </para>
        /// <para>
        /// После регистрации модуль автоматически добавляется в коллекцию активных, если:
        /// <list type="bullet">
        /// <item><description>Модуль включен в настройках приложения</description></item>
        /// <item><description>Модуль поддерживается текущей операционной системой</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Событие <see cref="ModuleRegistered"/> вызывается только если не активен флаг подавления событий
        /// (например, при вызове из <see cref="RegisterModules(IEnumerable{IModule})"/>).
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="module"/> равен <c>null</c>.</exception>
        public void RegisterModule(IModule module)
        {
            ArgumentNullException.ThrowIfNull(module);

            lock (_lock)
            {
                if (!_registeredModules.Add(module))
                {
                    if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    {
                        _logger.LogWarning("Module \"{ModuleName}\" is already registered.", module.Title);
                    }
                    return;
                }

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation("Module \"{ModuleName}\" registered successfully.", module.Title);
                }

                // Проверяем, должен ли модуль быть активирован
                if (IsEnabled(module) && IsSupported(module, OSDetector.DetectCurrentOS()))
                {
                    _activatedModules.Add(module);
                }
            }

            if (!_suppressEvents)
            {
                ModuleRegistered?.Invoke(module);
            }
        }

        /// <summary>
        /// Регистрирует несколько модулей атомарно с подавлением одиночных уведомлений.
        /// </summary>
        /// <param name="modules">Коллекция модулей для регистрации. Не может быть <c>null</c>.</param>
        /// <remarks>
        /// <para>
        /// Метод использует флаг <see cref="_suppressEvents"/> для предотвращения генерации события
        /// <see cref="ModuleRegistered"/> для каждого модуля. Вместо этого после завершения регистрации
        /// вызывается одно событие <see cref="ModulesBulkRegistered"/>.
        /// </para>
        /// <para>
        /// Это оптимизирует производительность UI, избегая множественных перерисовок Blazor-компонентов.
        /// </para>
        /// <para>
        /// Модули со значением <c>null</c> в коллекции игнорируются с логированием предупреждения.
        /// Дубликаты модулей также игнорируются.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="modules"/> равен <c>null</c>.</exception>
        public void RegisterModules(IEnumerable<IModule> modules)
        {
            ArgumentNullException.ThrowIfNull(modules);

            List<IModule> modulesList = [.. modules];
            if (modulesList.Count == 0)
            {
                return;
            }

            List<IModule> registeredModules = [];

            lock (_lock)
            {
                _suppressEvents = true;
                try
                {
                    foreach (IModule? module in modulesList)
                    {
                        if (module is null)
                        {
                            continue;
                        }

                        if (!_registeredModules.Add(module))
                        {
                            if (_logger?.IsEnabled(LogLevel.Warning) == true)
                            {
                                _logger.LogWarning("Module \"{ModuleName}\" is already registered. Skipping.", module.Title);
                            }
                            continue;
                        }

                        if (IsEnabled(module) && IsSupported(module, OSDetector.DetectCurrentOS()))
                        {
                            _activatedModules.Add(module);
                        }

                        registeredModules.Add(module);
                    }
                }
                finally
                {
                    _suppressEvents = false;
                }
            }

            if (registeredModules.Count > 0)
            {
                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation("Bulk registration completed. {Count} modules registered.", registeredModules.Count);
                }

                ModulesBulkRegistered?.Invoke(registeredModules.AsReadOnly());
            }
        }

        /// <summary>
        /// Регистрирует несколько модулей, переданных как параметры.
        /// </summary>
        /// <param name="modules">Массив модулей для регистрации.</param>
        /// <remarks>
        /// Это удобная перегрузка для регистрации нескольких модулей через запятую:
        /// <code>
        /// manager.RegisterModules(module1, module2, module3);
        /// </code>
        /// </remarks>
        public void RegisterModules(params IModule[] modules)
        {
            RegisterModules((IEnumerable<IModule>)modules);
        }

        /// <summary>
        /// Включает модуль, делая его видимым и доступным для пользователя.
        /// </summary>
        /// <param name="module">Модуль для включения. Не может быть <c>null</c>.</param>
        /// <remarks>
        /// <para>
        /// Метод изменяет состояние маршрута модуля в <see cref="IAppSettingsService"/> и добавляет
        /// модуль в коллекцию активных.
        /// </para>
        /// <para>
        /// Если модуль уже включен, метод не выполняет никаких действий.
        /// </para>
        /// <para>
        /// После успешного включения вызывается событие <see cref="ModuleStateChanged"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="module"/> равен <c>null</c>.</exception>
        public void EnableModule(IModule module)
        {
            ArgumentNullException.ThrowIfNull(module);

            lock (_lock)
            {
                if (!_registeredModules.Contains(module))
                {
                    if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    {
                        _logger.LogWarning("Cannot enable module \"{ModuleName}\": not registered.", module.Title);
                    }
                    return;
                }

                if (!_appSettingsService.IsRouteEnabled(module.BaseRoute))
                {
                    _appSettingsService.ToggleRoute(module.BaseRoute);
                    _activatedModules.Add(module);

                    if (_logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        _logger.LogInformation("Module \"{ModuleName}\" enabled.", module.Title);
                    }
                }
            }

            ModuleStateChanged?.Invoke(module);
        }

        /// <summary>
        /// Выключает модуль, скрывая его из пользовательского интерфейса.
        /// </summary>
        /// <param name="module">Модуль для выключения. Не может быть <c>null</c>.</param>
        /// <remarks>
        /// <para>
        /// Метод изменяет состояние маршрута модуля в <see cref="IAppSettingsService"/> и удаляет
        /// модуль из коллекции активных.
        /// </para>
        /// <para>
        /// Если модуль уже выключен, метод не выполняет никаких действий.
        /// </para>
        /// <para>
        /// После успешного выключения вызывается событие <see cref="ModuleStateChanged"/>.
        /// </para>
        /// <para>
        /// <b>Важно:</b> Модуль остается зарегистрированным в памяти. Для полного освобождения ресурсов
        /// требуется перезапуск приложения.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="module"/> равен <c>null</c>.</exception>
        public void DisableModule(IModule module)
        {
            ArgumentNullException.ThrowIfNull(module);

            lock (_lock)
            {
                if (!_registeredModules.Contains(module))
                {
                    if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    {
                        _logger.LogWarning("Cannot disable module \"{ModuleName}\": not registered.", module.Title);
                    }
                    return;
                }

                if (_appSettingsService.IsRouteEnabled(module.BaseRoute))
                {
                    _appSettingsService.ToggleRoute(module.BaseRoute);
                    _activatedModules.Remove(module);

                    if (_logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        _logger.LogInformation("Module \"{ModuleName}\" disabled.", module.Title);
                    }
                }
            }

            ModuleStateChanged?.Invoke(module);
        }

        /// <summary>
        /// Возвращает копию коллекции всех зарегистрированных модулей.
        /// </summary>
        /// <returns>Коллекция только для чтения, содержащая все зарегистрированные модули.</returns>
        /// <remarks>
        /// Возвращается копия коллекции, поэтому изменения в возвращаемой коллекции не влияют
        /// на внутреннее состояние менеджера. Метод потокобезопасен.
        /// </remarks>
        public IReadOnlyCollection<IModule> GetRegisteredModules()
        {
            lock (_lock)
            {
                return _registeredModules.AsReadOnly();
            }
        }

        /// <summary>
        /// Возвращает копию коллекции активных модулей (включенных и поддерживаемых текущей ОС).
        /// </summary>
        /// <returns>Коллекция только для чтения, содержащая активные модули.</returns>
        /// <remarks>
        /// <para>
        /// Активный модуль — это модуль, который:
        /// <list type="bullet">
        /// <item><description>Зарегистрирован в менеджере</description></item>
        /// <item><description>Включен в настройках приложения</description></item>
        /// <item><description>Поддерживается текущей операционной системой</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Эта коллекция должна использоваться для построения UI (сайдбара, навигационного меню).
        /// </para>
        /// <para>
        /// Возвращается копия коллекции, поэтому изменения в возвращаемой коллекции не влияют
        /// на внутреннее состояние менеджера. Метод потокобезопасен.
        /// </para>
        /// </remarks>
        public IReadOnlyCollection<IModule> GetActiveModules()
        {
            lock (_lock)
            {
                return _activatedModules.AsReadOnly();
            }
        }

        /// <summary>
        /// Проверяет, включен ли модуль в настройках приложения.
        /// </summary>
        /// <param name="module">Модуль для проверки. Не может быть <c>null</c>.</param>
        /// <returns><c>true</c>, если модуль включен; иначе <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="module"/> равен <c>null</c>.</exception>
        public bool IsEnabled(IModule module)
        {
            ArgumentNullException.ThrowIfNull(module);
            return _appSettingsService.IsRouteEnabled(module.BaseRoute);
        }

        /// <summary>
        /// Проверяет, поддерживается ли модуль указанной операционной системой.
        /// </summary>
        /// <param name="module">Модуль для проверки. Не может быть <c>null</c>.</param>
        /// <param name="os">Операционная система для проверки.</param>
        /// <returns><c>true</c>, если модуль поддерживается указанной ОС; иначе <c>false</c>.</returns>
        /// <remarks>
        /// Метод использует побитовую операцию <c>&amp;</c> для проверки, предполагая, что
        /// <see cref="OSPlatforms"/> является флаговым перечислением (<c>[Flags]</c>).
        /// Это позволяет модулю поддерживать несколько ОС одновременно.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="module"/> равен <c>null</c>.</exception>
        public bool IsSupported(IModule module, OSPlatforms os)
        {
            ArgumentNullException.ThrowIfNull(module);
            return (module.SupportedOS & os) == os;
        }

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                lock (_lock)
                {
                    _registeredModules.Clear();
                    _activatedModules.Clear();
                }

                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger.LogDebug("ModuleManager disposed. All module references cleared.");
                }
            }

            _disposed = true;
        }

        #endregion
    }
}