using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OmniCore.Core.Entities;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Modules.FMMS.Constants;
using OmniCore.Modules.FMMS.Interfaces;
using OmniCore.Modules.FMMS.Pages;
using OmniCore.Modules.FMMS.Resources.Languages;
using OmniCore.Modules.FMMS.Services;

namespace OmniCore.Modules.FMMS
{
    public sealed class FmmsModule(
        IStringLocalizer<FmmsResources> localizer,
        ILogger<FmmsModule>? logger = null) : IModule, IModuleSettingsProvider
    {
        private readonly IStringLocalizer<FmmsResources> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        private readonly ILogger<FmmsModule>? _logger = logger;
        private FmmsSettingsService? _settingsService;

        #region IModule
        public string Title => _localizer["ModuleName"];
        public string Description => _localizer["ModuleDescription"];
        public string Icon => MudBlazor.Icons.Material.Filled.PermMedia;
        public string BaseRoute => ModuleConstants.BasePagePath;
        public OSPlatforms SupportedOS { get; init; } = OSPlatforms.Windows;
        public bool Initialized { get; private set; } = false;
        public bool HideInNavMenu { get; } = false;

        public void AddModuleServices(IServiceCollection services)
        {
            services.AddLocalization();
            services.AddSingleton<FmmsSettingsService>();
            services.AddSingleton<IFileScannerService, FileScannerService>();
            services.AddSingleton<IDirectoryScannerService, DirectoryScannerService>();
            services.AddScoped<IClipboardService, ClipboardService>();

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger.LogInformation("FMMS module services registered successfully.");
            }
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            _settingsService = serviceProvider.GetRequiredService<FmmsSettingsService>();
            Initialized = true;

            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("FMMS module initialized with settings service.");
            }
        }

        public IReadOnlyList<INavigationItem> GetNavigationItems()
        {
            return
            [
                new NavigationItem(
                    _localizer["Page_Files_Scanner_Title"],
                    _localizer["Page_Files_Scanner_Description"],
                    MudBlazor.Icons.Material.Filled.Dashboard,
                    "files_scanner",
                    OSPlatforms.Windows,
                    true
                ),
                new NavigationItem(
                    _localizer["Page_Folder_Scanner_Title"],
                    _localizer["Page_Folder_Scanner_Description"],
                    MudBlazor.Icons.Material.Filled.FilterNone,
                    "directory_scanner",
                    OSPlatforms.Windows,
                    true
                )
            ];
        }
        #endregion

        #region IModuleSettingsProvider
        public string ModuleName => _localizer["ModuleName"];
        public Type SettingsComponentType => typeof(FmmsSettingsView);

        public async Task ResetToDefaultsAsync()
        {
            try
            {
                if (!Initialized || _settingsService is null)
                {
                    throw new InvalidOperationException("Module is not initialized. Call Initialize() first.");
                }

                await _settingsService.ResetToDefaultsAsync().ConfigureAwait(false);

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation("FMMS settings reset to defaults successfully.");
                }
            }
            catch (Exception ex)
            {
                if (_logger?.IsEnabled(LogLevel.Error) == true)
                {
                    _logger.LogError(ex, "Failed to reset FMMS settings to defaults.");
                }
                throw;
            }
        }
        #endregion
    }
}