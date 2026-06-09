using System.Windows;
using MusicerChord.Databases;
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

            // DB クラスの登録
            containerRegistry.Register<MyDbContext>();
            containerRegistry.Register<ISoundFileRepository, SoundFileRepository>();
            containerRegistry.Register<IListenHistoryRepository, ListenHistoryRepository>();
            containerRegistry.Register<SoundFileService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // DIコンテナから MyDbContext を取り出して EnsureCreated を実行する
            using var context = Container.Resolve<MyDbContext>();

            #if DEBUG
            // デバッグ起動時のみ、毎回DBをリセットして初期化する
            context.Database.EnsureDeleted();
            #endif

            context.Database.EnsureCreated();
        }
    }
}