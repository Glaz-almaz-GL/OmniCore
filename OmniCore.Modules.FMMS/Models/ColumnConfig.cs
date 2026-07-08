using OmniCore.Modules.FMMS.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.FMMS.Models
{
    /// <summary>
    /// Конфигурация колонки для динамического рендеринга в MudDataGrid
    /// </summary>
    /// <param name="HeaderKey">Ключ локализации для заголовка колонки</param>
    /// <param name="ValueSelector">Функция для извлечения значения из модели файла</param>
    public record ColumnConfig(
        string HeaderKey,
        Func<ScannedFile, string> ValueSelector
    );

}
