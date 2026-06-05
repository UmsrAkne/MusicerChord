using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundListViewModel : BindableBase
    {
        private readonly SoundPlayerService playerService;
        private ObservableCollection<SoundFile> soundFiles = new ();

        public SoundListViewModel(SoundPlayerService playerService)
        {
            this.playerService = playerService;
        }

        public ObservableCollection<SoundFile> SoundFiles
        {
            get => soundFiles;
            set => SetProperty(ref soundFiles, value);
        }

        public DelegateCommand PlayCommand => new DelegateCommand(() =>
        {
            playerService.Play();
        });

        public DelegateCommand StopCommand => new DelegateCommand(() =>
        {
            playerService.Stop();
        });

        public void UpdateSoundList(string objAbsolutePath)
        {
            var list = Directory.GetFiles(objAbsolutePath, "*.mp3", SearchOption.AllDirectories)
                .Select(p => new SoundFile() { RelativePath = p, });

            SoundFiles = new ObservableCollection<SoundFile>(list);
        }
    }
}