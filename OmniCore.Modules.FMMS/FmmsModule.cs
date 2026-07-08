using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OmniCore.Core.Entities;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Hybrid.Abstractions.Services;
using OmniCore.Hybrid.Abstractions.Interfaces;
using OmniCore.Modules.FMMS.Constants;
using OmniCore.Modules.FMMS.Interfaces;
using OmniCore.Modules.FMMS.Pages;
using OmniCore.Modules.FMMS.Resources.Languages;
using OmniCore.Modules.FMMS.Services;
using MudBlazor.Services;

namespace OmniCore.Modules.FMMS
{
    public sealed class FmmsModule(
        IStringLocalizer<FmmsResources> localizer,
        ILogger<FmmsModule>? logger = null) : IModule, IModuleSettingsProvider
    {
        private FmmsSettingsService? _settingsService;

        #region IModule
        /// <inheritdoc/>
        public string Title => localizer["ModuleName"];

        /// <inheritdoc/>
        public string Description => localizer["ModuleDescription"];

        /// <inheritdoc/>
        public string Icon => MudBlazor.Icons.Material.Filled.PermMedia;

        /// <inheritdoc/>
        public string BaseRoute => ModuleConstants.BasePagePath;

        /// <inheritdoc/>
        public OSPlatforms SupportedOS => OSPlatforms.Windows;

        /// <inheritdoc/>
        public bool Initialized { get; private set; } = false;

        /// <inheritdoc/>
        public bool HideInNavMenu => false;


        private bool _servicesRegistered = false;

        /// <inheritdoc/>
        public void AddModuleServices(IServiceCollection services)
        {
            services.AddMudServices();
            services.AddLocalization();
            services.AddSingleton<FmmsSettingsService>();
            services.AddSingleton<IFileScannerService, FileScannerService>();
            services.AddSingleton<IDirectoryScannerService, DirectoryScannerService>();
            services.AddSingleton<IFilePageService, FilePageService>();
            services.AddScoped<IClipboardService, ClipboardService>();

            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger.LogInformation("FMMS module services registered successfully.");
            }

            _servicesRegistered = true;
        }

        /// <inheritdoc/>
        public void Initialize(IServiceProvider serviceProvider)
        {
            if (!_servicesRegistered)
            {
                throw new InvalidOperationException("FMMS services are not registered.");
            }

            _settingsService = serviceProvider.GetRequiredService<FmmsSettingsService>();
            Initialized = true;

            if (logger?.IsEnabled(LogLevel.Debug) == true)
            {
                logger.LogDebug("FMMS module initialized with settings service.");
            }

            _settingsService.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public IReadOnlyList<INavigationItem> GetNavigationItems()
        {
            return
            [
                new NavigationItem(
                    localizer["Page_Files_Scanner_Title"],
                    localizer["Page_Files_Scanner_Description"],
                    MudBlazor.Icons.Material.Filled.Dashboard,
                    "files_scanner",
                    OSPlatforms.Windows,
                    true
                ),
                new NavigationItem(
                    localizer["Page_Folder_Scanner_Title"],
                    localizer["Page_Folder_Scanner_Description"],
                    MudBlazor.Icons.Material.Filled.FilterNone,
                    "directory_scanner",
                    OSPlatforms.Windows,
                    true
                )
            ];
        }
        #endregion

        #region IModuleSettingsProvider
        public string ModuleName => localizer["ModuleName"];
        public Type SettingsComponentType => typeof(FmmsSettingsView);

        /// <inheritdoc/>
        public async Task ResetToDefaultsAsync()
        {
            try
            {
                if (!Initialized || _settingsService is null)
                {
                    throw new InvalidOperationException("Module is not initialized. Call Initialize() first.");
                }

                await _settingsService.ResetToDefaultsAsync().ConfigureAwait(false);

                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger.LogInformation("FMMS settings reset to defaults successfully.");
                }
            }
            catch (Exception ex)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                {
                    logger.LogError(ex, "Failed to reset FMMS settings to defaults.");
                }
                throw;
            }
        }
        #endregion
    }
}