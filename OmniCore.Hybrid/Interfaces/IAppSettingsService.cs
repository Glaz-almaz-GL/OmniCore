using OmniCore.Hybrid.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Hybrid.Interfaces
{
    public interface IAppSettingsService
    {
        AppSettings Settings { get; }
        Task LoadAsync();
        Task SaveAsync();
        bool IsRouteEnabled(string route);
        void ToggleRoute(string route);
    }
}
