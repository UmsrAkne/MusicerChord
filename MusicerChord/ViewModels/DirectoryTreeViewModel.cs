using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DirectoryTreeViewModel : BindableBase
    {
        private ObservableCollection<ISoundContainer> soundContainers = new ();
        private AsyncRelayCommand<DirectorySoundSource> loadCommand;

        public ObservableCollection<ISoundContainer> SoundContainers
        {
            get => soundContainers;
            private set => SetProperty(ref soundContainers, value);
        }

        public ICommand LoadChildrenCommand => loadCommand ??= new AsyncRelayCommand<DirectorySoundSource>(async (d) =>
        {
            // xaml の方に記述はないが、このコマンドの引数には DirectorySoundSource が入力される。
            // CustomTreeView は展開された TreeViewItem の DataContext である　DirectorySoundSource を CommandParameter として渡す。
            // 詳細は CustomTreeView.xaml.cs の内部実装を参照。
            if (d != null)
            {
                if (!d.HasChildren)
                {
                    return;
                }

                var items = await Task.Run(() => SoundSourceFactory.CreateFromPath(d.AbsolutePath));
                d.Children.Clear();
                d.Children.AddRange(items);
            }
        });

        public async Task LoadDirectories(string rootPath)
        {
            var containers =
                await Task.Run(() => SoundSourceFactory.CreateFromPath(rootPath));

            SoundContainers = new ObservableCollection<ISoundContainer>(containers);
        }
    }
}