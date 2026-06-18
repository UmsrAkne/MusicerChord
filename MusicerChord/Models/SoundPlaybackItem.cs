using System;
using MusicerChord.Core;
using NAudio.Wave;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class SoundPlaybackItem : BindableBase
    {
        private PlaybackState playbackState;
        private bool isPlaying;
        private TimeSpan currentPlaybackTime;
        private readonly IMetadataReader metadataReader;

        public SoundPlaybackItem(SoundFile soundFile, IMetadataReader metadataReader = null)
        {
            SoundFile = soundFile;
            this.metadataReader = metadataReader;
            DisplayName = InitializeDisplayName();
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (SoundFile != null)
                {
                    SoundFile.Playing = value;
                }

                SetProperty(ref isPlaying, value);
            }
        }

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

        public string DisplayName { get; }

        private string InitializeDisplayName()
        {
            if (SoundFile == null)
            {
                return string.Empty;
            }

            // メタデータリーダーが渡されており、絶対パスが取得できる場合のみタイトルを読み込む
            if (metadataReader != null && !string.IsNullOrEmpty(SoundFile.FullPath))
            {
                var title = metadataReader.GetFileTitle(SoundFile.FullPath);
                if (!string.IsNullOrEmpty(title))
                {
                    return title; // メタデータのタイトルを返す
                }
            }

            // メタデータが無い、またはリーダーが無い場合はファイル名をフォールバックとして返す
            return SoundFile.FileNameWithoutExtension ?? string.Empty;
        }

        public override string ToString()
        {
            return $"[SoundPlaybackItem] FileName: {SoundFile.FileName}";
        }
    }
}