using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Modules.FMMS.Interfaces
{
    public interface IClipboardService
    {
        Task<string> ReadTextAsync();

        Task WriteTextAsync(string text);
    }
}
