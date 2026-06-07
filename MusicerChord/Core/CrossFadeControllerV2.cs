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
        private Queue<ISoundPlayer> activePlayers = new ();

        public CrossFadeControllerV2(ISoundPlayerFactory factory)
        {
            soundPlayerFactory = factory;
        }

        public event Action NextTrackRequested;

        public double CrossfadeDurationSeconds { get; set; } = 10.0;

        public double StartSeconds { get; set; } = 5.0;

        public double EndOffsetSeconds { get; set; } = 5.0;

        public bool IsPlaying => NowPlaying();

        public void Play(SoundPlaybackItem newItem)
        {
            Console.WriteLine($"Play(v2) {newItem.SoundFile.FileName}");
            var toActivePlayer = soundPlayerFactory.Create();
            toActivePlayer.PlaybackStopped += OnPlaybackStopped;

            toActivePlayer.Play(newItem);
            activePlayers.Enqueue(toActivePlayer);
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            var p = activePlayers.Dequeue();
            p.PlaybackStopped -= OnPlaybackStopped;

            Console.WriteLine($"----");
            Console.WriteLine("OnPlaybackStopped");

            // 他に再生中のプレイヤーがある場合はクロスフェードの最中なので、次を要求する必要はない。
            if (!NowPlaying())
            {
                NextTrackRequested?.Invoke();
                Console.WriteLine($"----");
                Console.WriteLine("NextTrackRequested (Triggered by PlaybackStopped)");
            }
        }

        public void Update(double deltaTimeSeconds)
        {
            // キューにプレイヤーがいない（何も再生していない）場合は何もしない
            if (!activePlayers.TryPeek(out var activePlayer) || !activePlayer.IsPlaying)
            {
                return;
            }

            // 1. UI等への状態同期
            activePlayer.UpdateItemState();

            // 音量のコントロール
            ApplyVolumeControl();

            // 2. 再生時間の監視 と 次の曲の要求ロジック
            if (CanExecuteCrossfade(activePlayer.CurrentItem))
            {
                double totalMs = activePlayer.GetTotalTimeMs();
                double currentMs = activePlayer.GetPlaybackTimeMs();

                // 終了地点（ミリ秒換算）
                var triggerThresholdMs = totalMs - ((EndOffsetSeconds + CrossfadeDurationSeconds) * 1000);

                // 終了地点に達した、かつ、まだ次の曲を要求していない場合
                if (currentMs >= triggerThresholdMs && !activePlayer.HasNextTrackRequested)
                {
                    activePlayer.HasNextTrackRequested = true;
                    NextTrackRequested?.Invoke();

                    Console.WriteLine($"----");
                    Console.WriteLine($"activePlayer.CurrentItem: {activePlayer.CurrentItem?.SoundFile?.FileName}");
                    Console.WriteLine($"activePlayer.CurrentItem.DurationMs: {activePlayer.CurrentItem?.SoundFile?.DurationMs}");

                    Console.WriteLine("NextTrackRequested (Triggered by Update)");
                }
            }
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

        private bool NowPlaying()
        {
            return activePlayers.Any();
        }

        private void ApplyVolumeControl()
        {
            var playerCount = activePlayers.Count;

            // プレイヤーの数が 0,1 の場合は音量固定
            if (playerCount < 2)
            {
                if (activePlayers.TryPeek(out var singlePlayer))
                {
                    singlePlayer.Volume = 1.0f; // または設定されたマスター音量
                }

                return;
            }

            // プレイヤー数２個の場合はクロスフェード
            if (playerCount == 2)
            {
                // Queueの性質上、先頭(Old)がフェードアウト、後ろ(New)がフェードインになる
                var playerArray = activePlayers.ToArray();
                var oldPlayer = playerArray[0];
                var newPlayer = playerArray[1];

                // 古い曲の残り時間をベースに、フェードの進捗率（0.0 〜 1.0）を計算
                double totalMs = oldPlayer.GetTotalTimeMs();
                double currentMs = oldPlayer.GetPlaybackTimeMs();

                // フェードが開始されるべき時間（ミリ秒）
                var fadeStartMs = totalMs - ((EndOffsetSeconds + CrossfadeDurationSeconds) * 1000);
                var fadeDurationMs = CrossfadeDurationSeconds * 1000;

                // 進捗率の計算 (0.0: フェード開始時 -> 1.0: フェード完了/曲終了時)
                var rawProgress = (currentMs - fadeStartMs) / fadeDurationMs;
                var progress = (float)Math.Clamp(rawProgress, 0.0, 1.0);

                // 音量の適用（線形フェードの場合）
                oldPlayer.Volume = 1.0f - progress; // 1.0 -> 0.0 へ減少
                newPlayer.Volume = progress; // 0.0 -> 1.0 へ増加
            }
        }
    }
}