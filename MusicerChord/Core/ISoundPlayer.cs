using System;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public interface ISoundPlayer
    {
        /// <summary>
        /// 音量（0.0 ～ 1.0）を取得または設定します。
        /// </summary>
        float Volume { get; set; }

        bool IsPlaying { get; }

        SoundPlaybackItem CurrentItem { get; }

        /// <summary>
        /// 対象のアイテムを読み込んで再生します。既に再生中の場合は停止して切り替えます。
        /// </summary>
        void Play(SoundPlaybackItem item, double startSeconds = 0);

        /// <summary>
        /// 再生を一時停止します。
        /// </summary>
        void Pause();

        /// <summary>
        /// 再生を完全に停止します。
        /// </summary>
        void Stop();

        /// <summary>
        /// 現在の再生位置を強制的に変更（シーク）します。
        /// </summary>
        void SetPlaybackTime(TimeSpan time);

        /// <summary>
        /// SoundPlaybackItem のプロパティを現在の NAudio の状態と同期します。
        /// </summary>
        void UpdateItemState();

        /// <summary>
        /// 内部リソースの停止と解放。
        /// </summary>
        void StopAndRelease();

        int GetPlaybackTimeMs();

        int GetTotalTimeMs();
    }
}