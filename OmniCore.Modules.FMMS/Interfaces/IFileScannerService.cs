using OmniCore.Modules.FMMS.Models;
using OmniCore.Modules.FMMS.Resources.Settings;

namespace OmniCore.Modules.FMMS.Interfaces
{
    public interface IFileScannerService
    {
        Task ScanDirectoryAsync(
            string directoryPath,
            FilesScanningSettings settings,
            IProgress<double> progress,
            Action<ScannedFile> onFileScanned,
            CancellationToken cancellationToken);
    }
}
