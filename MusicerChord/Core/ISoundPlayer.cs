using System;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public interface ISoundPlayer
    {
        event EventHandler PlaybackStopped;

        /// <summary>
        /// 音量（0.0 ～ 1.0）を取得または設定します。
        /// </summary>
        float Volume { get; set; }

        bool IsPlaying { get; }

        SoundPlaybackItem CurrentItem { get; }

        bool HasNextTrackRequested { get; set; }

        void Play(SoundPlaybackItem item, double startSeconds = 0);

        void Pause();

        void Stop();

        void SetPlaybackTime(TimeSpan time);

        void UpdateItemState();

        void StopAndRelease();

        int GetPlaybackTimeMs();

        int GetTotalTimeMs();
    }
}