using OmniCore.Core.Enums;
using System.Runtime.InteropServices;

namespace OmniCore.Hybrid.Extensions
{
    public static class OSConverter
    {
        public static readonly OSPlatform Android = OSPlatform.Create("ANDROID");
        public static readonly OSPlatform iOS = OSPlatform.Create("IOS");

        public static OSPlatforms ToFlags(this OSPlatform oSPlatform)
        {
            return oSPlatform.ToString() switch
            {
                "WINDOWS" => OSPlatforms.Windows,
                "LINUX" => OSPlatforms.Linux,
                "FREEBSD" => OSPlatforms.FreeBSD,
                "OSX" => OSPlatforms.OSX,
                "ANDROID" => OSPlatforms.Android,
                "IOS" => OSPlatforms.iOS,
                _ => OSPlatforms.Unknown
            };
        }

        public static OSPlatforms ToFlags(this OSPlatform? oSPlatform)
        {
            return oSPlatform?.ToFlags() ?? OSPlatforms.None;
        }

        public static OSPlatform[] FromFlags(this OSPlatforms oSPlatforms)
        {
            if (oSPlatforms == OSPlatforms.None)
            {
                return [];
            }

            List<OSPlatform> result = [];

            if (oSPlatforms.HasFlag(OSPlatforms.Windows))
            {
                result.Add(OSPlatform.Windows);
            }

            if (oSPlatforms.HasFlag(OSPlatforms.Linux))
            {
                result.Add(OSPlatform.Linux);
            }

            if (oSPlatforms.HasFlag(OSPlatforms.FreeBSD))
            {
                result.Add(OSPlatform.FreeBSD);
            }

            if (oSPlatforms.HasFlag(OSPlatforms.OSX))
            {
                result.Add(OSPlatform.OSX);
            }

            if (oSPlatforms.HasFlag(OSPlatforms.Android))
            {
                result.Add(Android);
            }

            if (oSPlatforms.HasFlag(OSPlatforms.iOS))
            {
                result.Add(iOS);
            }

            return [.. result];
        }

        public static OSPlatform[]? FromFlags(this OSPlatforms? oSPlatforms)
        {
            return oSPlatforms?.FromFlags();
        }
    }
}
