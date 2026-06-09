using System;
using System.Windows.Threading;

namespace MusicerChord.Core
{
    /// <summary>
    /// SoundListViewModel に配置され、音声の再生制御ロジックを提供するサービスクラスです。
    /// </summary>
    public class SoundPlayerService
    {
        private readonly DispatcherTimer timer = new ();
        private readonly int updateIntervalMs = 100;

        private DateTime lastUpdateTime = DateTime.Now;

        public SoundPlayerService()
        {
            CrossfadeController = new CrossFadeControllerV2(new SoundPlayerFactory());
            CrossfadeController.NextTrackRequested += PlayNext;

            timer.Interval = TimeSpan.FromMilliseconds(updateIntervalMs);
            timer.Tick += (_, _) =>
            {
                var now = DateTime.Now;
                CrossfadeController.Update((now - lastUpdateTime).TotalSeconds);
                lastUpdateTime = now;
            };
        }

        public SoundPlaylist SoundPlaylist { get; set; }

        private ICrossfadeController CrossfadeController { get; set; }

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
                return;
            }

            if (CrossfadeController.CanExecuteCrossfade(nextItem))
            {
                CrossfadeController.Play(nextItem, 0);
                SoundPlaylist.MoveNext();
            }
        }
    }
}