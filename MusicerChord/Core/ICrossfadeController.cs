using System;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public interface ICrossfadeController
    {
        event Action NextTrackRequested;

        double CrossfadeDurationSeconds { get; set; }

        double StartSeconds { get; set; }

        double EndOffsetSeconds { get; set; }

        bool IsPlaying { get; }

        /// <summary>
        /// 新しい曲を再生します。すでに再生中の曲がある場合はクロスフェードを試みます。
        /// </summary>
        void Play(SoundPlaybackItem newItem);

        /// <summary>
        /// 定周期（タイマー等）から呼び出される更新処理。
        /// テスタブルにするため、前回の呼び出しからの経過時間（deltaTime）を引数で受け取ります。
        /// </summary>
        /// <param name="deltaTimeSeconds">前回のフレーム/タイマー呼び出しからの経過秒数（例: 0.1）</param>
        void Update(double deltaTimeSeconds);

        /// <summary>
        /// 指定されたアイテムが、設定されたクロスフェード要素の合計時間を満たしているか判定します。
        /// </summary>
        bool CanExecuteCrossfade(SoundPlaybackItem item);

        void StopAll();
    }
}