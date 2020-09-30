using MahApps.Metro.Controls;

using MiniApps.SpaghettiUI.Constants;

using Prism.Regions;

namespace MiniApps.SpaghettiUI.Views
{
    public partial class ShellWindow : MetroWindow
    {
        public ShellWindow(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager.SetRegionName(hamburgerMenuContentControl, Regions.Main);
            RegionManager.SetRegionManager(hamburgerMenuContentControl, regionManager);
        }
    }
}
