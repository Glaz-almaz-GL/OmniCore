using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;

namespace OmniCore.Hybrid.Interfaces
{
    public interface IModuleManager : IDisposable
    {
        event Action<IModule>? ModuleRegistered;
        event Action<IModule>? ModuleStateChanged;

        event Action<IReadOnlyCollection<IModule>>? ModulesBulkRegistered;

        /// <summary>
        /// Возвращает все модули, зарегистрированные в системе.
        /// </summary>
        IReadOnlyCollection<IModule> GetRegisteredModules();

        /// <summary>
        /// Возвращает только те модули, которые включены пользователем и поддерживаются текущей ОС.
        /// </summary>
        IReadOnlyCollection<IModule> GetActiveModules();

        /// <summary>
        /// Возвращает модуль по его базовому маршруту.
        /// </summary>
        IModule? GetModuleByRoute(string baseRoute);

        bool IsEnabled(IModule module);

        bool IsSupported(IModule module, OSPlatforms os);

        /// <summary>
        /// Включает модуль.
        /// </summary>
        void EnableModule(IModule module);

        /// <summary>
        /// Выключает модуль.
        /// </summary>
        void DisableModule(IModule module);

        /// <summary>
        /// Регистрирует уже созданный экземпляр модуля в менеджере.
        /// </summary>
        void RegisterModule(IModule module);

        /// <summary>
        /// Регистрирует несколько модулей атомарно.
        /// </summary>
        void RegisterModules(IEnumerable<IModule> modules);

        /// <summary>
        /// Регистрирует несколько модулей атомарно.
        /// </summary>
        void RegisterModules(params IModule[] modules);
    }
}
