using Microsoft.Extensions.Logging;
using OmniCore.Modules.FMMS.Interfaces;
using OmniCore.Modules.FMMS.Models;
using OmniCore.Modules.FMMS.Resources.Settings;
using System.Runtime.CompilerServices;

namespace OmniCore.Modules.FMMS.Services
{
    internal sealed class FileScannerService(ILogger<FileScannerService>? logger = null) : IFileScannerService
    {
        private readonly ILogger<FileScannerService>? _logger = logger;

        private readonly EnumerationOptions _enumerationOptions = new()
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            ReturnSpecialDirectories = false
        };

        public async IAsyncEnumerable<ScannedFile> ScanDirectoryAsync(
            string directoryPath,
            FilesScanningSettings settings,
            IProgress<double> progress,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IEnumerable<string> filePaths;
            try
            {
                filePaths = Directory.EnumerateFiles(directoryPath, "*.*", _enumerationOptions);
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }

            int processedFiles = 0;
            int dirPathLength = Path.TrimEndingDirectorySeparator(directoryPath).Length + 1;

            foreach (string filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ScannedFile? scannedFile = null;

                try
                {
                    scannedFile = await ProcessFileAsync(filePath, dirPathLength, settings, cancellationToken).ConfigureAwait(false);

                    await CalculateHashes(settings, scannedFile, filePath, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                {
                    _logger?.LogError(ex, "File skipped: \"{FilePath}\".", filePath);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Unexpected error: \"{FilePath}\".", filePath);
                }

                if (scannedFile is not null)
                {
                    yield return scannedFile;
                }

                processedFiles++;
                progress?.Report(processedFiles);
            }
        }

        private async Task CalculateHashes(FilesScanningSettings settings, ScannedFile scannedFile, string filePath, CancellationToken cancellationToken)
        {
            if (settings.CalculateMD5)
            {
                scannedFile.MD5 = await FileHashService.ComputeMD5Async(filePath, cancellationToken).ConfigureAwait(false);

                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger?.LogDebug("MD5 Hash calculated: \"{MD5}\"", scannedFile.MD5);
                }
            }

            if (settings.CalculateSHA256)
            {
                scannedFile.SHA256 = await FileHashService.ComputeSHA256Async(filePath, cancellationToken).ConfigureAwait(false);

                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger?.LogDebug("SHA256 Hash calculated: \"{SHA256}\"", scannedFile.SHA256);
                }
            }

            if (settings.CalculateSHA512)
            {
                scannedFile.SHA512 = await FileHashService.ComputeSHA512Async(filePath, cancellationToken).ConfigureAwait(false);

                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger?.LogDebug("SHA512 Hash calculated: \"{SHA512}\"", scannedFile.SHA512);
                }
            }
        }

        private async Task<ScannedFile> ProcessFileAsync(string filePath, int dirPathLength, FilesScanningSettings settings, CancellationToken cancellationToken = default)
        {
            FileInfo fileInfo = new(filePath);

            // Substring эффективнее
            string relativeFilePath = filePath.Length > dirPathLength
                ? filePath[dirPathLength..]
                : string.Empty;

            ScannedFile result = new()
            {
                Name = relativeFilePath,
                Extension = fileInfo.Extension.ToLowerInvariant(),
                FullPath = fileInfo.FullName,
                Size = fileInfo.Length,
                IsArchive = settings.CustomArchiveExtensions.Contains(fileInfo.Extension.ToLowerInvariant().TrimStart('.'))
            };

            // Подсчет страниц (синхронная операция чтения файла)
            if (result.Extension == ".pdf")
            {
                try
                {
                    result.PagesCount = await Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return FilePageService.GetPagesCountInPdf(filePath);
                    }, cancellationToken).ConfigureAwait(false);

                    if (_logger?.IsEnabled(LogLevel.Debug) == true)
                    {
                        _logger?.LogDebug("PDF pages count for {FilePath}: {Count}", filePath, result.PagesCount);
                    }
                }
                catch (Exception ex)
                {
                    result.PagesCount = -1;
                    _logger?.LogError(ex, "File page count error for \"{FilePath}\"; Returned -1", filePath);
                }
            }
            else if (settings.PagesCountCustomRules.TryGetValue(result.Extension, out int customPages))
            {
                result.PagesCount = customPages;
            }

            return result;
        }
    }
}