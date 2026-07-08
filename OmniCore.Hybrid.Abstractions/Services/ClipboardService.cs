using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using OmniCore.Hybrid.Abstractions.Interfaces;

namespace OmniCore.Hybrid.Abstractions.Services
{
    public sealed class ClipboardService(IJSRuntime jsRuntime, ILogger<ClipboardService>? logger = null) : IClipboardService
    {
        private readonly ILogger<ClipboardService>? _logger = logger;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public async Task<string> ReadTextAsync()
        {
            string text = await _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText").ConfigureAwait(false);

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation("Read text from clipboard: \"{Text}\"", text);
            }

            return text;
        }

        public async Task WriteTextAsync(string text)
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text).ConfigureAwait(false);

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation("Copied text to clipboard: \"{Text}\"", text);
            }
        }
    }
}
