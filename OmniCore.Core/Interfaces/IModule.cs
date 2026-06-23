using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OmniCore.Core.Enums;

namespace OmniCore.Core.Interfaces
{
    public interface IModule
    {
        string Title { get; }
        string Description { get; }
        string Icon { get; }
        string BaseRoute { get; }
        OSPlatforms SupportedOS { get; }
        bool Initialized { get; }
        bool HideInNavMenu { get; }

        IReadOnlyList<INavigationItem> GetNavigationItems();

        void AddModuleServices(IServiceCollection services);
        void Initialize(IServiceProvider serviceProvider);
    }
}