using OmniCore.Modules.FMMS.Interfaces;
using OmniCore.Modules.FMMS.Models;
using OmniCore.Modules.FMMS.Resources.Settings;

namespace OmniCore.Modules.FMMS.Services
{
    internal sealed class FileScannerService : IFileScannerService
    {
        public async Task ScanDirectoryAsync(
            string directoryPath,
            FilesScanningSettings settings,
            IProgress<double> progress,
            Action<ScannedFile> onFileScanned,
            CancellationToken cancellationToken)
        {
            string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            int totalFiles = files.Length;
            int processedFiles = 0;
            int dirPathLength = directoryPath.Length + 1;

            foreach (string filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileInfo fileInfo = new(filePath);
                string relativeFilePath;
                //relativeFilePath = Path.GetRelativePath(directoryPath, filePath); // Слишком неэффективный из-за неоднакратного вычисления

                relativeFilePath = filePath[dirPathLength..]; // Используем substring с длиной пути папки

                ScannedFile scannedFile = new()
                {
                    Name = relativeFilePath,
                    Extension = fileInfo.Extension.ToLowerInvariant(),
                    FullPath = fileInfo.FullName,
                    Size = fileInfo.Length,
                    IsArchive = settings.CustomArchiveExtensions.Contains(fileInfo.Extension.ToLowerInvariant().TrimStart('.'))
                };

                // 1. Подсчет страниц (упрощенный пример для PDF и кастомных правил)
                if (scannedFile.Extension == ".pdf")
                {
                    try { scannedFile.PagesCount = FilePageService.GetPagesCountInPdf(filePath); }
                    catch { scannedFile.PagesCount = -1; } // Ошибка чтения
                }
                else if (settings.PagesCountCustomRules.TryGetValue(scannedFile.Extension, out int customPages))
                {
                    scannedFile.PagesCount = customPages; // Заглушка для кастомных правил
                }

                // 2. Вычисление хешей (только если включено в настройках)
                if (settings.CalculateMD5)
                {
                    scannedFile.MD5 = await FileHashService.ComputeMD5Async(filePath, cancellationToken);
                }

                if (settings.CalculateSHA256)
                {
                    scannedFile.SHA256 = await FileHashService.ComputeSHA256Async(filePath, cancellationToken);
                }

                if (settings.CalculateSHA512)
                {
                    scannedFile.SHA512 = await FileHashService.ComputeSHA512Async(filePath, cancellationToken);
                }

                // Передаем файл в UI
                onFileScanned(scannedFile);

                // Обновляем прогресс
                processedFiles++;
                progress.Report((double)processedFiles / totalFiles * 100.0);
            }
        }
    }
}