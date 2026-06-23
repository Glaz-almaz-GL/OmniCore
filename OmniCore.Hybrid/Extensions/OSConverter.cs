using OmniCore.Core.Enums;
using System.Runtime.InteropServices;

namespace OmniCore.Hybrid.Extensions
{
    public static class OSConverter
    {
        public static readonly OSPlatform Android = OSPlatform.Create("ANDROID");
        public static readonly OSPlatform iOS = OSPlatform.Create("IOS");
        public static readonly OSPlatform Unknown = OSPlatform.Create("UNKNOWN");
        public static readonly OSPlatform All = OSPlatform.Create("All");

        public static OSPlatforms ToFlags(this OSPlatform oSPlatform)
        {
            if (oSPlatform == OSPlatform.Windows) return OSPlatforms.Windows;
            if (oSPlatform == OSPlatform.Linux) return OSPlatforms.Linux;
            if (oSPlatform == OSPlatform.FreeBSD) return OSPlatforms.FreeBSD;
            if (oSPlatform == OSPlatform.OSX) return OSPlatforms.OSX;
            if (oSPlatform == Android) return OSPlatforms.Android;
            if (oSPlatform == iOS) return OSPlatforms.iOS;
            if (oSPlatform == All) return OSPlatforms.All;

            return OSPlatforms.Unknown;
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

            if (oSPlatforms.HasFlag(OSPlatforms.All))
            {
                result.Add(All);
            }

            if (result.Count == 0)
            {
                result.Add(Unknown);
            }

            return [.. result];
        }

        public static OSPlatform[]? FromFlags(this OSPlatforms? oSPlatforms)
        {
            return oSPlatforms?.FromFlags();
        }
    }
}
