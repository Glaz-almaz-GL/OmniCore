using System.Globalization;

namespace OmniCore.Shared.UI.Models
{
    public record AppSettings
    {
        public CultureInfo Culture { get; set; } = new CultureInfo("ru-RU");
        public ThemeMode ThemeMode { get; set; }
    }
}
