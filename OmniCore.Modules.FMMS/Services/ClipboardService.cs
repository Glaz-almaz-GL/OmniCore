using Microsoft.JSInterop;
using OmniCore.Modules.FMMS.Interfaces;

namespace OmniCore.Modules.FMMS.Services
{
    internal sealed class ClipboardService(IJSRuntime jsRuntime) : IClipboardService
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public async Task<string> ReadTextAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }

        public async Task WriteTextAsync(string text)
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
