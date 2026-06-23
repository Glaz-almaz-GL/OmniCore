using MudBlazor;
using OmniCore.Modules.TextEditor.Abstractions.Interfaces;
using OmniCore.Modules.TextEditor.Pages;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.TextEditor.Editors
{
    public sealed class MarkdownTextEditor : ITextEditor
    {
        public string Title { get; } = "Markdown";
        public string Icon { get; } = Icons.Custom.FileFormats.FileDocument;
        public Type TextEditorType { get; } = typeof(MarkdownEditorPage);
        public IReadOnlyList<string> SupportedExtensions { get; } = [".md", ".markdown", ".txt"];
        public int Priority { get; } = 10;
    }
}
