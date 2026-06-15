using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Core.Entities
{
    public sealed record NavigationItem(string Title, string Description, string Icon, string RelativeRoute, OSPlatforms SupportedOS, bool IsEnabled) : INavigationItem;
}
