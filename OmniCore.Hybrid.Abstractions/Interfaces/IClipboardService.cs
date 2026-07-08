namespace OmniCore.Hybrid.Abstractions.Interfaces
{
    public interface IClipboardService
    {
        Task<string> ReadTextAsync();

        Task WriteTextAsync(string text);
    }
}
