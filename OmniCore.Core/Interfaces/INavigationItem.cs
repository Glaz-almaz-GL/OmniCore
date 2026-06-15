using OmniCore.Core.Enums;

namespace OmniCore.Core.Interfaces
{
    public interface INavigationItem
    {
        string Title { get; }
        string Description { get; }
        string Icon { get; }
        string RelativeRoute { get; }
        OSPlatforms SupportedOS { get; }
        bool IsEnabled { get; }
    }
}
