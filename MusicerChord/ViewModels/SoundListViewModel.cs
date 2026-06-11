using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MusicerChord.Core;
using MusicerChord.Databases;
using MusicerChord.Models;
using MusicerChord.Utils;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundListViewModel : BindableBase
    {
        private readonly SoundPathResolver soundPathResolver;
        private readonly SoundFileService soundFileService;
        private ObservableCollection<SoundFile> soundFiles = new ();

        public SoundListViewModel(SoundPlayerService playerService, SoundFileService soundFileService, bool isDesignMode = false)
        {
            SoundPlayerService = playerService;
            this.soundFileService = soundFileService;

            // IO処理が含まれていると、XAMLプレビューが無効になるため。
            if (!isDesignMode)
            {
                var appSettings = AppSettings.Load(AppSettings.SettingFilePath);
                soundPathResolver = new SoundPathResolver(appSettings.RootPath);
            }
        }

        public ObservableCollection<SoundFile> SoundFiles
        {
            get => soundFiles;
            set => SetProperty(ref soundFiles, value);
        }

        public SoundPlayerService SoundPlayerService { get; set; }

        public DelegateCommand PlayCommand => new DelegateCommand(() =>
        {
            SoundPlayerService.Play();
        });

        public DelegateCommand StopCommand => new DelegateCommand(() =>
        {
            SoundPlayerService.Stop();
        });

        public DelegateCommand ReverseSoundListCommand => new DelegateCommand(() =>
        {
            var list = SoundFiles.Reverse().ToList();
            SetUpSoundFilesAndPlaylist(list);
        });

        public DelegateCommand ShuffleSoundListCommand => new DelegateCommand(() =>
        {
            var list = SoundFiles.OrderBy(_ => Guid.NewGuid()).ToList();
            SetUpSoundFilesAndPlaylist(list);
        });

        public DelegateCommand SortPlayCountAscendingCommand => new DelegateCommand(() =>
        {
            var list = SoundFiles.OrderBy(s => s.PlayCount).ToList();
            SetUpSoundFilesAndPlaylist(list);
        });

        public async Task UpdateSoundListAsync(string objAbsolutePath)
        {
            // 1. まずはファイル名のリストだけ高速に作成（Durationはまだ0）
            var list = Directory.GetFiles(objAbsolutePath, "*.mp3", SearchOption.AllDirectories)
                .Select(p => new SoundFile()
                {
                    RelativePath = soundPathResolver.ResolveRelativePath(p),
                    FullPath = p,
                })
                .ToList();

            // 2. 画面とプレイヤーサービスに即座にセット（ユーザーを待たせない）
            SetUpSoundFilesAndPlaylist(list);

            // 3. 重いデータはバックグラウンドで後からロード
            await Task.Run(async () =>
            {
                // サービス層を呼び出す。DB検索（超高速）＋未登録分のみファイル解析（重い）
                await soundFileService.InitializeFileMetadataAsync(list);
            });
        }

        private void SetUpSoundFilesAndPlaylist(List<SoundFile> list)
        {
            SoundFiles = new ObservableCollection<SoundFile>(list);
            var index = 1;
            foreach (var soundPlaybackItem in list)
            {
                soundPlaybackItem.LineNumber = index++;
            }

            SoundPlayerService.SoundPlaylist = new SoundPlaylist(SoundFiles.Select(f => new SoundPlaybackItem(f)).ToList());
        }
    }
}