using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    /// <summary>
    /// SoundListViewModel に配置され、音声の再生制御ロジックを提供するサービスクラスです。
    /// </summary>
    public class SoundPlayerService
    {
        private readonly DispatcherTimer timer = new ();
        private readonly int updateIntervalMs = 100;

        private int currentPlayingIndex;
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

        public List<SoundPlaybackItem> SoundPlaybackItems { get; set; }

        private ICrossfadeController CrossfadeController { get; set; }

        public void Play()
        {
            currentPlayingIndex = 0;
            timer.Start();
            CrossfadeController.StopAll();
            CrossfadeController.Play(SoundPlaybackItems[0]);
        }

        public void Stop()
        {
            currentPlayingIndex = 0;
            CrossfadeController.StopAll();
            timer.Stop();
        }

        private void PlayNext()
        {
            if (SoundPlaybackItems == null || !SoundPlaybackItems.Any())
            {
                return;
            }

            var oldIndex = currentPlayingIndex;
            currentPlayingIndex = currentPlayingIndex >= SoundPlaybackItems.Count - 1 ? -1 : currentPlayingIndex;
            var item = SoundPlaybackItems[++currentPlayingIndex];

            if (!CrossfadeController.IsPlaying)
            {
                CrossfadeController.Play(item);
                Console.WriteLine($"PlayNext index: {currentPlayingIndex}");
                return;
            }

            if (CrossfadeController.CanExecuteCrossfade(item))
            {
                CrossfadeController.Play(item);
                Console.WriteLine($"PlayNext index: {currentPlayingIndex}");
                return;
            }

            currentPlayingIndex = oldIndex;
        }
    }
}