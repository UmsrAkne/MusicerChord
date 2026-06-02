using System.Windows;
using MusicerChord.ViewModels;
using MusicerChord.Views;
using Prism.Ioc;

namespace MusicerChord
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<DirectoryTreeViewModel>();
            containerRegistry.RegisterSingleton<SoundListViewModel>();
        }
    }
}