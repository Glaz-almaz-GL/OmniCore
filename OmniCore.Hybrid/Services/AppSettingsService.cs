using Microsoft.Extensions.Logging;
using OmniCore.Hybrid.Interfaces;
using OmniCore.Hybrid.Models;
using System.Globalization;
using System.Text.Json;

namespace OmniCore.Hybrid.Services
{
    public sealed partial class AppSettingsService : IAppSettingsService, IDisposable
    {
        private readonly string _filePath;
        private readonly ILogger<AppSettingsService>? _logger;
        private readonly SemaphoreSlim _saveLock = new(1, 1);

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private bool _disposed;

        public event Action<AppSettings>? OnSettingsChanged;
        public AppSettings Settings { get; private set; } = new();

        public AppSettingsService(ILogger<AppSettingsService>? logger = null)
        {
            _logger = logger;
            string appDataDirectory = FileSystem.AppDataDirectory;
            _filePath = Path.Combine(appDataDirectory, "app_settings.json");

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation("Settings service initialized. File path: {FilePath}", _filePath);
            }

            _ = LoadAsync().ConfigureAwait(false);
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    if (_logger?.IsEnabled(LogLevel.Warning) == true)
                    {
                        _logger?.LogWarning("Settings file not found at {FilePath}. Default settings will be used.", _filePath);
                    }
                    return;
                }

                string json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
                AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                if (loaded != null)
                {
                    Settings = loaded;
                    ValidateSettings();
                    ApplyCultureInfo();

                    if (_logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        _logger?.LogInformation("Settings loaded successfully. Language: {Language}, Disabled routes count: {Count}", Settings.Language, Settings.DisabledRoutes.Count);
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                {
                    _logger?.LogError(jsonEx, "Settings deserialization error. File is corrupted.");
                }
                HandleCorruptedFile();
            }
            catch (OperationCanceledException ex)
            {
                if (_logger?.IsEnabled(LogLevel.Warning) == true)
                {
                    _logger?.LogWarning(ex, "Settings loading was canceled.");
                }
            }
            catch (Exception ex)
            {
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                {
                    _logger?.LogError(ex, "Unexpected error during settings loading.");
                }
                Settings = new AppSettings();
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _saveLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                string json = JsonSerializer.Serialize(Settings, _jsonOptions);

                // Create directory if it doesn't exist
                string? directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(_filePath, json, cancellationToken).ConfigureAwait(false);

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger?.LogInformation("Settings saved successfully. Language: {Language}, Disabled routes count: {Count}", Settings.Language, Settings.DisabledRoutes.Count);
                }

                // Notify subscribers about changes
                ApplyCultureInfo();
                OnSettingsChanged?.Invoke(Settings);
            }
            catch (OperationCanceledException ex)
            {
                if (_logger?.IsEnabled(LogLevel.Warning) == true)
                {
                    _logger?.LogWarning(ex, "Settings saving was canceled.");
                }
            }
            catch (Exception ex)
            {
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                {
                    _logger?.LogError(ex, "Critical error during settings saving.");
                }
            }
            finally
            {
                _saveLock.Release();
            }
        }

        private void ApplyCultureInfo()
        {
            var settingsCulture = new CultureInfo(Settings.Language);
            CultureInfo.CurrentCulture = settingsCulture;
            CultureInfo.CurrentUICulture = settingsCulture;
            CultureInfo.DefaultThreadCurrentCulture = settingsCulture;
            CultureInfo.DefaultThreadCurrentUICulture = settingsCulture;
        }

        public bool IsRouteEnabled(string route)
        {
            return !Settings.DisabledRoutes.Contains(route);
        }

        public void ToggleRoute(string route)
        {
            if (IsRouteEnabled(route))
            {
                Settings.DisabledRoutes.Add(route);
                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger?.LogDebug("Route {Route} disabled.", route);
                }
            }
            else
            {
                Settings.DisabledRoutes.Remove(route);
                if (_logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    _logger?.LogDebug("Route {Route} enabled.", route);
                }
            }
        }

        /// <summary>
        /// Method to handle corrupted settings file.
        /// </summary>
        private void HandleCorruptedFile()
        {
            try
            {
                string backupPath = $"{_filePath}.bak_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Move(_filePath, backupPath);
                if (_logger?.IsEnabled(LogLevel.Warning) == true)
                {
                    _logger?.LogWarning("Corrupted settings file renamed to {BackupPath}. Default settings created.", backupPath);
                }
            }
            catch (Exception ex)
            {
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                {
                    _logger?.LogError(ex, "Failed to create backup of corrupted settings file.");
                }
            }

            Settings = new AppSettings();
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(Settings.Language))
            {
                Settings.Language = CultureInfo.CurrentCulture.Name;
                _logger?.LogWarning("Invalid language setting. Using current culture: {Language}", Settings.Language);
            }

            try
            {
                _ = new CultureInfo(Settings.Language);
            }
            catch (CultureNotFoundException ex)
            {
                _logger?.LogWarning(ex, "Unknown culture {Language}. Using default.", Settings.Language);
                Settings.Language = "en-US";
            }

            if (Settings.DisabledRoutes == null)
            {
                Settings.DisabledRoutes = [];
            }
        }

        public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
        {
            Settings = new AppSettings();
            await SaveAsync(cancellationToken).ConfigureAwait(false);

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation("Settings reset to defaults.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _saveLock.Dispose();
            }

            _disposed = true;
        }
    }
}