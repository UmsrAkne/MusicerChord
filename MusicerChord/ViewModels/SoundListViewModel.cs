using System.Collections.ObjectModel;
using MusicerChord.Models;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundListViewModel : BindableBase
    {
        private ObservableCollection<SoundFile> soundFiles = new ();

        public ObservableCollection<SoundFile> SoundFiles
        {
            get => soundFiles;
            set => SetProperty(ref soundFiles, value);
        }
    }
}