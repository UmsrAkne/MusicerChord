using System;
using System.Collections.Generic;
using System.Linq;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public class CrossFadeControllerV2 : ICrossfadeController
    {
        private List<ISoundPlayer> players;
        private List<ISoundPlayer> activePlayers = new ();

        public CrossFadeControllerV2(ISoundPlayer p1, ISoundPlayer p2, ISoundPlayer p3)
        {
            players = new List<ISoundPlayer>() { p1, p2, p3, };
            foreach (var soundPlayer in players)
            {
                soundPlayer.PlaybackStopped += (_, _) =>
                {
                    NextTrackRequested?.Invoke();
                };
            }
        }

        public event Action NextTrackRequested;

        public double CrossfadeDurationSeconds { get; set; } = 10.0;

        public double StartSeconds { get; set; } = 5.0;

        public double EndOffsetSeconds { get; set; } = 5.0;

        public void Play(SoundPlaybackItem newItem)
        {
            var toActivePlayer = MoveToActivePlayer();
            toActivePlayer.Play(newItem);
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

        private ISoundPlayer MoveToActivePlayer()
        {
            var p = players.FirstOrDefault();
            activePlayers.Add(p);
            players.Remove(p);
            return p;
        }
    }
}