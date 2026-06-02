using System;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class SoundPlaybackItem : BindableBase
    {
        private PlaybackState playbackState;
        private bool isPlaying;

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

        public TimeSpan CurrentPlaybackTime { get; set; }

        public SoundFile SoundFile { get; set; }
    }
}