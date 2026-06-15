using Microsoft.Extensions.Logging;
using OmniCore.Modules.FMMS.Resources.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OmniCore.Modules.FMMS.Services
{
    internal sealed class FmmsSettingsService
    {
        private readonly ILogger<FmmsSettingsService>? _logger;
        private readonly string _filePath;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public FmmsSettingsService(ILogger<FmmsSettingsService>? logger = null)
        {
            _logger = logger;
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appDataDirectory, "OmniCore", "fmms_settings.json");

            Settings = new FilesScanningSettings();
        }

        /// <summary>
        /// Текущие активные настройки. UI должен работать именно с этим свойством.
        /// </summary>
        public FilesScanningSettings Settings { get; private set; }

        /// <summary>
        /// Загружает настройки из файла. Должен быть вызван один раз при старте приложения.
        /// </summary>
        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = await File.ReadAllTextAsync(_filePath);
                    FilesScanningSettings? loadedSettings = JsonSerializer.Deserialize<FilesScanningSettings>(json, _jsonOptions);

                    if (loadedSettings != null)
                    {
                        // Просто присваиваем загруженные настройки свойству
                        Settings = loadedSettings;
                        if (_logger?.IsEnabled(LogLevel.Information) == true)
                        {
                            _logger?.LogInformation("FMMS settings loaded successfully from {Path}", _filePath);
                        }

                        return;
                    }
                }

                _logger?.LogInformation("FMMS settings file not found. Using defaults.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading FMMS settings. Default settings will be used.");
            }
        }

        /// <summary>
        /// Сохраняет ТЕКУЩИЕ настройки (Settings) в файл.
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                // Убеждаемся, что директория существует
                string? directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(Settings, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger?.LogInformation("FMMS settings saved successfully to {Path}", _filePath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving FMMS settings");
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Сбрасывает настройки к дефолтным и сохраняет их.
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            Settings = new FilesScanningSettings();
            await SaveAsync();
        }
    }
}