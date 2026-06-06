using MusicerChord.Models;
using MusicerChord.Utils;

namespace MusicerChord.Core
{
    using System;

    public class CrossfadeController
    {
        // 2つのプレイヤーインスタンス
        private readonly ISoundPlayer playerA;
        private readonly ISoundPlayer playerB;

        // 現在の役割分担
        private ISoundPlayer activePlayer;
        private ISoundPlayer fadingOutPlayer;

        // クロスフェードの進行状態管理
        private bool isCrossfading;
        private double crossfadeElapsedSeconds;

        public CrossfadeController(ISoundPlayer playerA, ISoundPlayer playerB)
        {
            this.playerA = playerA ?? throw new ArgumentNullException(nameof(playerA));
            this.playerB = playerB ?? throw new ArgumentNullException(nameof(playerB));

            playerA.PlaybackStopped += OnPlaybackStopped;
            playerB.PlaybackStopped += OnPlaybackStopped;

            // 初期状態はPlayerAをメインに設定
            activePlayer = this.playerA;
            fadingOutPlayer = this.playerB;
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            var p1 = sender as ISoundPlayer;
            var p2 = p1 == playerA ? playerB : playerA;

            p1?.UpdateItemState();
            p2?.UpdateItemState();

            if (!p1!.IsPlaying && !p2!.IsPlaying)
            {
                NextTrackRequested?.Invoke();
            }
        }

        // 次の曲への切り替え要求を通知するイベント（外部のプレイリスト管理者がこれをフックして Play() を呼ぶ）
        public event Action CrossfadeTimingReached;

        public event Action NextTrackRequested;

        public double CrossfadeDurationSeconds { get; set; } = 10.0;

        public double StartSeconds { get; set; } = 5.0;

        public double EndOffsetSeconds { get; set; } = 5.0;

        /// <summary>
        /// 新しい曲を再生します。すでに再生中の曲がある場合はクロスフェードを試みます。
        /// </summary>
        public void Play(SoundPlaybackItem newItem)
        {
            if (newItem == null)
            {
                return;
            }

            // 1. クロスフェードが有効な条件を満たしているかチェック
            var canCrossfade = CanExecuteCrossfade(newItem);

            if (activePlayer.IsPlaying && canCrossfade)
            {
                // --- クロスフェードを伴う遷移 ---
                // 現在のメインプレイヤーを「退場用」に回す
                fadingOutPlayer = activePlayer;

                // もう片方のプレイヤーを「入場用（アクティブ）」にする
                activePlayer = activePlayer == playerA ? playerB : playerA;

                // 新しい曲を指定の開始位置から、音量ゼロで再生開始
                activePlayer.Volume = 0f;
                activePlayer.Play(newItem, StartSeconds);

                // クロスフェード状態の初期化
                isCrossfading = true;
                crossfadeElapsedSeconds = 0.0;
            }
            else
            {
                var isCurrentlyPlaying = activePlayer.IsPlaying;

                // --- クロスフェード無効（初回再生、または曲が短すぎる場合） ---
                StopAll();

                activePlayer.Volume = 1.0f;

                // 「今再生中で、かつクロスフェード可能な曲」の時だけ StartSeconds を適用する。
                // すでに再生が止まっていればどんな場合でも 0 秒から再生する。
                var actualStart = (isCurrentlyPlaying && canCrossfade) ? StartSeconds : 0.0;
                activePlayer.Play(newItem, actualStart);
            }
        }

        /// <summary>
        /// 定周期（タイマー等）から呼び出される更新処理。
        /// テスタブルにするため、前回の呼び出しからの経過時間（deltaTime）を引数で受け取ります。
        /// </summary>
        /// <param name="deltaTimeSeconds">前回のフレーム/タイマー呼び出しからの経過秒数（例: 0.1）</param>
        public void Update(double deltaTimeSeconds)
        {
            // 1. UI等への状態同期
            activePlayer.UpdateItemState();
            if (fadingOutPlayer.IsPlaying)
            {
                fadingOutPlayer.UpdateItemState();
            }

            // 2. クロスフェード中の音量変化ロジック
            if (isCrossfading)
            {
                crossfadeElapsedSeconds += deltaTimeSeconds;

                // 進行度（0.0 ～ 1.0）
                var progress = (float)(crossfadeElapsedSeconds / CrossfadeDurationSeconds);

                if (progress >= 1.0f)
                {
                    // フェード完了
                    activePlayer.Volume = 1.0f;
                    fadingOutPlayer.Stop();
                    isCrossfading = false;
                }
                else
                {
                    // 音量の線形変化（リニアフェード）
                    activePlayer.Volume = progress;
                    fadingOutPlayer.Volume = 1.0f - progress;
                }
            }

            // 3. 通常再生中：現在の曲が「終了地点」に達したかを監視
            else if (activePlayer.IsPlaying)
            {
                // 現在の曲がクロスフェード条件を満たしている場合のみ判定
                if (CanExecuteCrossfade(activePlayer.CurrentItem))
                {
                    double totalMs = activePlayer.GetTotalTimeMs();
                    double currentMs = activePlayer.GetPlaybackTimeMs();

                    // 終了地点（ミリ秒換算）
                    var triggerThresholdMs = totalMs - (EndOffsetSeconds * 1000);

                    if (currentMs >= triggerThresholdMs)
                    {
                        // 自動で次の曲へ移行するためのイベントなどを発火させる（今回はタイミング検知まで）
                        OnCrossfadeTimingReached();
                    }
                }
            }
        }

        /// <summary>
        /// 指定されたアイテムが、設定されたクロスフェード要素の合計時間を満たしているか判定します。
        /// </summary>
        private bool CanExecuteCrossfade(SoundPlaybackItem item)
        {
            if (item?.SoundFile == null)
            {
                return false;
            }

            var totalDurationSeconds = item.SoundFile.DurationMs / 1000.0;

            if (totalDurationSeconds <= 0)
            {
                // 再生前で時間が取れない場合は一旦有効として扱う、またはデータ側の値を使用する
                return true;
            }

            var requiredMinimumSeconds = StartSeconds + EndOffsetSeconds + CrossfadeDurationSeconds;

            return totalDurationSeconds >= requiredMinimumSeconds;
        }

        public void StopAll()
        {
            activePlayer.Stop();
            fadingOutPlayer.Stop();
            isCrossfading = false;
        }

        protected virtual void OnCrossfadeTimingReached()
        {
            CrossfadeTimingReached?.Invoke();
        }
    }
}