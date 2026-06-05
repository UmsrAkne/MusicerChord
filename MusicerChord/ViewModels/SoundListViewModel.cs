using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        public void UpdateSoundList(string objAbsolutePath)
        {
            var list = Directory.GetFiles(objAbsolutePath, "*.mp3", SearchOption.AllDirectories)
                .Select(p => new SoundFile() { RelativePath = p, });

            SoundFiles = new ObservableCollection<SoundFile>(list);
        }
    }
}