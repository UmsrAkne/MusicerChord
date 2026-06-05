using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MusicerChord.Core;
using MusicerChord.Models;
using MusicerChord.Utils;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundListViewModel : BindableBase
    {
        private readonly SoundPlayerService playerService;
        private readonly SoundPathResolver soundPathResolver;
        private ObservableCollection<SoundFile> soundFiles = new ();

        public SoundListViewModel(SoundPlayerService playerService)
        {
            this.playerService = playerService;

            var appSettings = AppSettings.Load(AppSettings.SettingFilePath);
            soundPathResolver = new SoundPathResolver(appSettings.RootPath);
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
            var list =
                Directory.GetFiles(objAbsolutePath, "*.mp3", SearchOption.AllDirectories)
                .Select(p => new SoundFile()
                {
                    RelativePath = soundPathResolver.ResolveRelativePath(p),
                    FullPath = p,
                });

            SoundFiles = new ObservableCollection<SoundFile>(list);
            playerService.SoundPlaybackItems = new List<SoundPlaybackItem>(SoundFiles.Select(f => new SoundPlaybackItem(f)));
        }
    }
}