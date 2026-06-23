using OmniCore.Hybrid.Models;

namespace OmniCore.Hybrid.Interfaces
{
    public interface IAppSettingsService
    {
        AppSettings Settings { get; }

        /// <summary>
        /// Событие, уведомляющее об изменении настроек.
        /// </summary>
        event Action<AppSettings>? OnSettingsChanged;

        Task LoadAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
        bool IsRouteEnabled(string route);
        void ToggleRoute(string route);

        /// <summary>
        /// Сбрасывает настройки к значениям по умолчанию.
        /// </summary>
        Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);

    }
}
