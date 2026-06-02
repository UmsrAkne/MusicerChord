using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DirectoryTreeViewModel : BindableBase
    {
        private ObservableCollection<ISoundContainer> soundContainers = new ();

        public ObservableCollection<ISoundContainer> SoundContainers
        {
            get => soundContainers;
            private set => SetProperty(ref soundContainers, value);
        }

        public async Task LoadDirectories(string rootPath)
        {
            var containers =
                await Task.Run(() => SoundSourceFactory.CreateFromPath(rootPath));

            SoundContainers = new ObservableCollection<ISoundContainer>(containers);
        }
    }
}