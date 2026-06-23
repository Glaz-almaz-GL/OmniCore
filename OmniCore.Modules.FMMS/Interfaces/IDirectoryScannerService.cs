using OmniCore.Modules.FMMS.Models;
using OmniCore.Modules.FMMS.Resources.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.FMMS.Interfaces
{
    public interface IDirectoryScannerService
    {
        IAsyncEnumerable<ScannedDirectory> ScanDirectoryAsync(
            string rootPath,
            DirectoryScanningSettings settings,
            IProgress<double> progress,
            CancellationToken cancellationToken = default);
    }
}
