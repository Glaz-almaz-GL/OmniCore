using Microsoft.Extensions.Logging;
using OmniCore.Hybrid.Interfaces;

namespace OmniCore.Hybrid.Services
{
    public class LayoutStateService(ILogger<LayoutStateService>? logger = null) : ILayoutStateService
    {
        private readonly ILogger<LayoutStateService>? _logger = logger;
        public event Action? OnStateChanged;

        public void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
            _logger?.LogInformation("A layer update was requested.");
        }
    }
}
