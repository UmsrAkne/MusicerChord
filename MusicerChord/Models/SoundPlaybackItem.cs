using System;
using NAudio.Wave;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class SoundPlaybackItem : BindableBase
    {
        private PlaybackState playbackState;
        private bool isPlaying;
        private TimeSpan currentPlaybackTime;

        public SoundPlaybackItem(SoundFile soundFile)
        {
            SoundFile = soundFile;
        }

        public bool IsPlaying { get => isPlaying; set => SetProperty(ref isPlaying, value); }

        public PlaybackState PlaybackState
        {
            get => playbackState;
            set => SetProperty(ref playbackState, value);
        }

        public TimeSpan CurrentPlaybackTime
        {
            get => currentPlaybackTime;
            set => SetProperty(ref currentPlaybackTime, value);
        }

        public SoundFile SoundFile { get; set; }
    }
}