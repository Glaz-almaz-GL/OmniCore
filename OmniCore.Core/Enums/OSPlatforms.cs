using System.Runtime.InteropServices;

namespace OmniCore.Core.Enums
{
    [Flags]
    public enum OSPlatforms
    {
        None = 0,
        Windows = 1 << 0,
        Linux = 1 << 1,
        OSX = 1 << 2,
        FreeBSD = 1 << 3,
        Android = 1 << 4,
        iOS = 1 << 5,
        Unknown = 1 << 6,

        All = Windows | Linux | OSX | Android | iOS | FreeBSD | Unknown
    }
}
