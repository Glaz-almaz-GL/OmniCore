using OmniCore.Modules.FMMS.Abstractions.Models;

namespace OmniCore.Modules.FMMS.Abstractions.Interfaces
{
    public interface IDirectoryScannerService
    {
        IAsyncEnumerable<ScannedDirectory> ScanDirectoryAsync(
            string rootPath,
            DirectoryScanningSettings settings,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
