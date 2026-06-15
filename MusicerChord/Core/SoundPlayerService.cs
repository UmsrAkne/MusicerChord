using System;
using System.Windows.Threading;
using MusicerChord.Databases;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    /// <summary>
    /// SoundListViewModel に配置され、音声の再生制御ロジックを提供するサービスクラスです。
    /// </summary>
    public class SoundPlayerService
    {
        private readonly DispatcherTimer timer;
        private readonly int updateIntervalMs = 100;
        private readonly SoundFileService soundFileService;

        private DateTime lastUpdateTime;

        public SoundPlayerService(
            SoundFileService soundFileService,
            ICrossfadeController crossfadeController,
            IDateTimeProvider dateTimeProvider,
            DispatcherTimer timer)
        {
            this.soundFileService = soundFileService;
            CrossfadeController = crossfadeController;
            CrossfadeController.NextTrackRequested += PlayNext;

            this.timer = timer;

            timer.Interval = TimeSpan.FromMilliseconds(updateIntervalMs);
            timer.Tick += (_, _) =>
            {
                var now = dateTimeProvider.Now;
                CrossfadeController.Update((now - lastUpdateTime).TotalSeconds);
                lastUpdateTime = now;
            };

            lastUpdateTime = dateTimeProvider.Now;
        }

        public SoundPlaylist SoundPlaylist { get; set; }

        public ICrossfadeController CrossfadeController { get; set; }

        public void Play()
        {
            if (SoundPlaylist is not { HasItems: true, })
            {
                return;
            }

            timer.Start();
            CrossfadeController.StopAll();

            var firstItem = SoundPlaylist.ResetToFirst();
            CrossfadeController.Play(firstItem, CrossfadeController.Volume);

            RecordPlayHistory(firstItem);
        }

        public void Stop()
        {
            SoundPlaylist.ResetToFirst();
            CrossfadeController.StopAll();
            timer.Stop();
        }

        private void PlayNext()
        {
            if (SoundPlaylist == null || !SoundPlaylist.HasItems)
            {
                return;
            }

            // 1. 次のアイテムを事前にチェック（この時点ではインデックスは進まない）
            var nextItem = SoundPlaylist.PeekNext();

            if (!CrossfadeController.IsPlaying)
            {
                CrossfadeController.Play(nextItem, CrossfadeController.Volume);
                SoundPlaylist.MoveNext();
                RecordPlayHistory(nextItem);
                return;
            }

            if (CrossfadeController.CanExecuteCrossfade(nextItem))
            {
                CrossfadeController.Play(nextItem, 0);
                RecordPlayHistory(nextItem);
                SoundPlaylist.MoveNext();
            }
        }

        private void RecordPlayHistory(SoundPlaybackItem item)
        {
            if (item?.SoundFile == null)
            {
                return;
            }

            // 投げっぱなしだが、エラーだけは受け取れるようにする。
            FireAndForget(item.SoundFile.Id);
            item.SoundFile.PlayCount++;
            return;

            async void FireAndForget(int soundFileId)
            {
                try
                {
                    await soundFileService.RecordListenHistoryAsync(soundFileId);
                }
                catch (Exception ex)
                {
                    // ここでログを吐く（例: Microsoft.Extensions.Logging など）
                    Console.WriteLine($"履歴の記録に失敗しました: {ex.Message}");
                }
            }
        }
    }
}