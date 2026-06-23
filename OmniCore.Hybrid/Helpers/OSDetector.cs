using OmniCore.Core.Enums;
using System.Runtime.InteropServices;

namespace OmniCore.Hybrid.Helpers
{
    public static class OSDetector
    {
        /// <summary>
        /// Определяет текущую операционную систему, на которой запущено приложение.
        /// </summary>
        /// <returns>Флаговое значение <see cref="OSPlatforms"/>, соответствующее текущей ОС.</returns>
        /// <remarks>
        /// Метод использует <see cref="RuntimeInformation.IsOSPlatform(OSPlatform)"/> для определения ОС.
        /// Возвращаемое значение может содержать несколько флагов, если приложение запущено в среде,
        /// поддерживающей несколько платформ (например, WSL).
        /// </remarks>
        public static OSPlatforms DetectCurrentOS()
        {
            OSPlatforms currentOS = OSPlatforms.None;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentOS |= OSPlatforms.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                currentOS |= OSPlatforms.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                currentOS |= OSPlatforms.OSX;
            }

            if (RuntimeInformation.IsOSPlatform(Extensions.OSConverter.Android))
            {
                currentOS |= OSPlatforms.Android;
            }

            if (RuntimeInformation.IsOSPlatform(Extensions.OSConverter.iOS))
            {
                currentOS |= OSPlatforms.iOS;
            }

            return currentOS;
        }
    }
}
