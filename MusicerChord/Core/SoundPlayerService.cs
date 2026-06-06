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
        private int currentPlayingIndex;

        public SoundPlayerService()
        {
            var p1 = new SoundPlayer();
            var p2 = new SoundPlayer();
            CrossfadeController = new CrossfadeController(p1, p2);
            CrossfadeController.CrossfadeTimingReached += () =>
            {
                if (SoundPlaybackItems == null || !SoundPlaybackItems.Any())
                {
                    return;
                }

                CrossfadeController.Play(SoundPlaybackItems[++currentPlayingIndex]);
            };

            CrossfadeController.NextTrackRequested += () =>
            {
                if (SoundPlaybackItems == null || !SoundPlaybackItems.Any())
                {
                    return;
                }

                CrossfadeController.Play(SoundPlaybackItems[++currentPlayingIndex]);
            };

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (sender, args) =>
            {
                CrossfadeController.Update(0.1);
            };
        }

        public List<SoundPlaybackItem> SoundPlaybackItems { get; set; }

        private CrossfadeController CrossfadeController { get; set; }

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
    }
}