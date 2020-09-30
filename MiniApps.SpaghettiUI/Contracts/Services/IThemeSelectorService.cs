using MiniApps.SpaghettiUI.Models;

namespace MiniApps.SpaghettiUI.Contracts.Services
{
    public interface IThemeSelectorService
    {
        bool SetTheme(AppTheme? theme = null);

        AppTheme GetCurrentTheme();
    }
}
