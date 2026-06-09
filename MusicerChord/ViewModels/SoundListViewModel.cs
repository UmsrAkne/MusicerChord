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
        private readonly SoundPlayerService playerService;
        private readonly SoundPathResolver soundPathResolver;
        private readonly SoundFileService soundFileService;
        private ObservableCollection<SoundFile> soundFiles = new ();

        public SoundListViewModel(SoundPlayerService playerService, SoundFileService soundFileService)
        {
            this.playerService = playerService;
            this.soundFileService = soundFileService;

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
            SoundFiles = new ObservableCollection<SoundFile>(list);
            playerService.SoundPlaylist = new SoundPlaylist(SoundFiles.Select(f => new SoundPlaybackItem(f)));

            // 3. 重いデータはバックグラウンドで後からロード
            await Task.Run(async () =>
            {
                // サービス層を呼び出す。DB検索（超高速）＋未登録分のみファイル解析（重い）
                await soundFileService.FillOrFetchDurationsAsync(list);
            });
        }
    }
}