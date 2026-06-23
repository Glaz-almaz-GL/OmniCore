using MudBlazor;
using OmniCore.Modules.TextEditor.Abstractions.Interfaces;
using OmniCore.Modules.TextEditor.Pages;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.TextEditor.Editors
{
    public sealed class JsonTextEditor : ITextEditor
    {
        public string Title { get; } = "Json";
        public string Icon { get; } = Icons.Custom.FileFormats.FileCode;
        public Type TextEditorType { get; } = typeof(JsonEditorPage);
        public IReadOnlyList<string> SupportedExtensions { get; } = [".json", ".txt"];
    }
}
