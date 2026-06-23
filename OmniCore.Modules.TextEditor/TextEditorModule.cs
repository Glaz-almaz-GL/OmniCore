using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor;
using OmniCore.Core.Entities;
using OmniCore.Core.Enums;
using OmniCore.Core.Interfaces;
using OmniCore.Modules.TextEditor.Abstractions.Interfaces;
using OmniCore.Modules.TextEditor.Editors;
using OmniCore.Modules.TextEditor.Pages;
using OmniCore.Modules.TextEditor.Resources.Languages;

namespace OmniCore.Modules.TextEditor;

public sealed class TextEditorModule(
    IStringLocalizer<TextEditorResources> localizer,
    ILogger<TextEditorModule>? logger = null) : IModule
{
    private readonly IStringLocalizer<TextEditorResources> _localizer = localizer;
    private readonly ILogger<TextEditorModule>? _logger = logger;

    public string Title => _localizer["ModuleName"];
    public string Description => _localizer["ModuleDescription"];
    public string Icon => Icons.Custom.FileFormats.FileDocument;
    public string BaseRoute => "text-editor";
    public OSPlatforms SupportedOS { get; init; } = OSPlatforms.All;
    public bool Initialized { get; private set; }
    public bool HideInNavMenu { get; } = false;

    public void AddModuleServices(IServiceCollection services)
    {
        services.AddLocalization();
        services.AddMudMarkdownServices();
        services.AddSingleton<ITextEditor, MarkdownTextEditor>();
        services.AddSingleton<ITextEditor, JsonTextEditor>();

        _logger?.LogInformation("TextEditor module services registered.");
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        Initialized = true;
        _logger?.LogDebug("TextEditor module initialized.");
    }

    public IReadOnlyList<INavigationItem> GetNavigationItems()
    {
        return
        [
            new NavigationItem(
                _localizer["Page_Editor_Title"],
                _localizer["Page_Editor_Description"],
                Icons.Material.Filled.EditNote,
                "editor",
                OSPlatforms.All,
                true
            )
        ];
    }
}