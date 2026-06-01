using System.Collections.ObjectModel;
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
            set => SetProperty(ref soundContainers, value);
        }
    }
}