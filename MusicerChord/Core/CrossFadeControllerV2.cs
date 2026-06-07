using System;
using System.Collections.Generic;
using System.Linq;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public class CrossFadeControllerV2 : ICrossfadeController
    {
        private readonly ISoundPlayerFactory soundPlayerFactory;
        private List<ISoundPlayer> players;
        private List<ISoundPlayer> activePlayers = new ();

        public CrossFadeControllerV2(ISoundPlayerFactory factory)
        {
            soundPlayerFactory = factory;
        }

        public event Action NextTrackRequested;

        public double CrossfadeDurationSeconds { get; set; } = 10.0;

        public double StartSeconds { get; set; } = 5.0;

        public double EndOffsetSeconds { get; set; } = 5.0;

        public void Play(SoundPlaybackItem newItem)
        {
            var toActivePlayer = soundPlayerFactory.Create();
            toActivePlayer.PlaybackStopped += (_, _) => { NextTrackRequested?.Invoke(); };
            toActivePlayer.Play(newItem);
            activePlayers.Add(toActivePlayer);
        }

        public void Update(double deltaTimeSeconds)
        {
            Console.WriteLine($"Update: {deltaTimeSeconds}");
        }

        public bool CanExecuteCrossfade(SoundPlaybackItem item)
        {
            if (item?.SoundFile == null)
            {
                return false;
            }

            var totalDurationSeconds = item.SoundFile.DurationMs / 1000.0;

            if (totalDurationSeconds <= 0)
            {
                // 不正の値の場合はフェード不可。
                return false;
            }

            var requiredMinimumSeconds = StartSeconds + EndOffsetSeconds + CrossfadeDurationSeconds;

            return totalDurationSeconds >= requiredMinimumSeconds;
        }

        public void StopAll()
        {
            Console.WriteLine("StopAll(v2)");
        }
    }
}