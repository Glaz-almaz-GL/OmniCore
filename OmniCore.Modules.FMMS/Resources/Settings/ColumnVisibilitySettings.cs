using OmniCore.Modules.FMMS.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.FMMS.Resources.Settings
{
    /// <summary>
    /// Настройки видимости колонок
    /// </summary>
    public sealed class ColumnVisibilitySettings
    {
        /// <summary>
        /// Видимость стандартных колонок
        /// </summary>
        public Dictionary<FileColumn, bool> StandardColumns { get; set; } = new()
        {
            { FileColumn.Id, true },
            { FileColumn.Name, true },
            { FileColumn.PagesCount, true },
            { FileColumn.Extension, true },
            { FileColumn.FullPath, false },
            { FileColumn.IsArchive, false },
            { FileColumn.IsArchiveEntry, false },
            { FileColumn.CompressedSize, false },
            { FileColumn.UnCompressedSize, false },
            { FileColumn.Size, true }
        };

        /// <summary>
        /// Видимость колонок хешей (ключ = имя алгоритма из HashingSettings.AlgorithmsToCalculate)
        /// </summary>
        /// <remarks>
        /// Колонки хешей формируются динамически на основе выбранных алгоритмов.
        /// Этот словарь позволяет пользователю скрывать ненужные колонки хешей.
        /// </remarks>
        public Dictionary<string, bool> HashColumns { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            { "SHA-256", true },
            { "MD5", false },
            { "SHA-512", false }
        };
    }
}
