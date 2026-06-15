using OmniCore.Hybrid.Interfaces;
using OmniCore.Hybrid.Models;
using System.Text.Json;
using System.Diagnostics;

namespace OmniCore.Hybrid.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly string _filePath;

        // Опции для красивого и стандартного JSON
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AppSettings Settings { get; private set; } = new();

        public AppSettingsService()
        {
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appDataDirectory, "OmniCore", "app_settings.json");

            Debug.WriteLine($"[AppSettings] Инициализация сервиса. Путь к файлу: {_filePath}");
        }

        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                    if (loaded != null)
                    {
                        Settings = loaded;
                        Debug.WriteLine($"[AppSettings] Настройки загружены. Язык: {Settings.Language}");
                    }
                }
                else
                {
                    Debug.WriteLine("[AppSettings] Файл настроек не найден, созданы настройки по умолчанию.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppSettings] Ошибка загрузки настроек: {ex.Message}");
                // В случае ошибки сбрасываем на дефолтные, чтобы приложение не падало
                Settings = new AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);

                Debug.WriteLine($"[AppSettings] Настройки сохранены. Язык: {Settings.Language}, Отключено маршрутов: {Settings.DisabledRoutes.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppSettings] Ошибка сохранения настроек: {ex.Message}");
            }
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
            }
            else
            {
                Settings.DisabledRoutes.Remove(route);
            }
        }
    }
}