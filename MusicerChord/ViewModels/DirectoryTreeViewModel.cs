using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DirectoryTreeViewModel : BindableBase
    {
        private ObservableCollection<ISoundContainer> soundContainers = new ();

        private ISoundContainer selectedContainer;

        public event Action<ISoundContainer> SoundContainerOpened ;

        public ObservableCollection<ISoundContainer> SoundContainers
        {
            get => soundContainers;
            private set => SetProperty(ref soundContainers, value);
        }

        public ISoundContainer SelectedContainer
        {
            get => selectedContainer;
            set => SetProperty(ref selectedContainer, value);
        }

        public DelegateCommand OpenSoundContainerCommand => new (() =>
        {
            Console.WriteLine("Open Sound Container");
            SoundContainerOpened?.Invoke(SelectedContainer);
        });

        public async Task LoadDirectories(string rootPath)
        {
            var containers =
                await Task.Run(() => SoundSourceFactory.CreateFromPath(rootPath));

            SoundContainers = new ObservableCollection<ISoundContainer>(containers);
        }
    }
}