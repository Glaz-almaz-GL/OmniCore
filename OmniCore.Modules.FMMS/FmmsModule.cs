using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
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
    public sealed class FmmsModule(IStringLocalizer<FmmsResources> localizer) : IModule, IModuleSettingsProvider
    {
        private readonly IStringLocalizer<FmmsResources> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        #region IModule
        public string Title => _localizer["ModuleName"];
        public string Description => _localizer["ModuleDescription"];
        public string Icon => MudBlazor.Icons.Material.Filled.PermMedia;
        public string BaseRoute => ModuleConstants.BasePagePath;
        public OSPlatforms SupportedOS { get; init; } = OSPlatforms.Windows;


        public void AddModuleServices(IServiceCollection services)
        {
            services.AddLocalization();
            services.AddSingleton<FmmsSettingsService>();
            services.AddSingleton<IFileScannerService>(new FileScannerService());
            services.AddScoped<IClipboardService, ClipboardService>();
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
                    "folder_scanner",
                    OSPlatforms.Windows,
                    true
                )
            ];
        }
        #endregion

        #region IModuleSettingsProvider
        public string ModuleName => _localizer["ModuleName"];

        public Type SettingsComponentType => typeof(FmmsSettingsView);

        public Task ResetToDefaultsAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}