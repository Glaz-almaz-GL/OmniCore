using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.TextEditor.Abstractions.Interfaces
{
    public interface ITextEditor
    {
        /// <summary>
        /// Отображаемое имя редактора
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Иконка редактора
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Тип Razor-компонента для рендеринга
        /// </summary>
        Type TextEditorType { get; }

        /// <summary>
        /// Поддерживаемые расширения файлов (опционально)
        /// </summary>
        IReadOnlyList<string> SupportedExtensions { get; }

        /// <summary>
        /// Приоритет отображения (меньше = раньше)
        /// </summary>
        int Priority => 0;
    }
}
