using System.Text.Json.Serialization;

namespace OmniCore.Hybrid.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "ru-RU";

        [JsonInclude]
        public HashSet<string> DisabledRoutes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}