using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MusicerChord.Core;
using MusicerChord.Databases;
using MusicerChord.Models;
using MusicerChord.Utils;
using NAudio.Wave;
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
            // 1. まずはファイル名のリストだけ作成。Duration などのデータは読まない。
            var list = Directory.GetFiles(objAbsolutePath, "*.mp3", SearchOption.AllDirectories)
                .Select(p => new SoundFile()
                {
                    RelativePath = soundPathResolver.ResolveRelativePath(p),
                    FullPath = p,
                })
                .ToList();

            // 2. 画面とプレイヤーサービスに即座にセット
            SoundFiles = new ObservableCollection<SoundFile>(list);
            playerService.SoundPlaylist = new SoundPlaylist(SoundFiles.Select(f => new SoundPlaybackItem(f)));

            // 3. バックグラウンドで非同期に再生時間をロード
            // await しますが、UIスレッドをブロックしないように Task.Run で別スレッドに逃がします
            await Task.Run(() =>
            {
                foreach (var file in list)
                {
                    // 各ファイルの時間をロード（将来ここは「DBにあればDBから、なければファイルから」になる）
                    var duration = LoadDuration(file.FullPath);

                    // ★ WPF / UIスレッドへ値を書き戻す処理
                    // （ObservableCollection や PropertyChanged を安全に同期するため、
                    // 必要に応じて Application.Current.Dispatcher.Invoke などを挟むとより安全です）
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        file.DurationMs = (int)duration;
                    });
                }
            });
        }

        private double LoadDuration(string fullPath)
        {
            try
            {
                // ファイルが存在しない場合はスルー
                if (!File.Exists(fullPath))
                {
                    return 0;
                }

                // AudioFileReader はメタデータではなく、オーディオストリームそのものを解析します。
                // ※内部でファイルを開くため、激重処理（I/O負荷＋デコード負荷）になります。
                using var reader = new AudioFileReader(fullPath);
                return reader.TotalTime.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                // 読み込みエラー（ファイル破損やロックなど）が発生した場合は、
                // ログを吐くなどして安全に 0 を返す
                System.Diagnostics.Debug.WriteLine($"NAudioによる時間取得に失敗: {fullPath} - {ex.Message}");
                return 0;
            }
        }
    }
}