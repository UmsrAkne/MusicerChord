using System;
using System.Collections.Generic;
using System.Linq;
using MusicerChord.Models;
using Prism.Mvvm;

namespace MusicerChord.Core
{
    public class CrossFadeControllerV2 : BindableBase, ICrossfadeController
    {
        private readonly ISoundPlayerFactory soundPlayerFactory;
        private readonly Queue<ISoundPlayer> activePlayers = new ();
        private double volume = 1.0;

        public CrossFadeControllerV2(ISoundPlayerFactory factory)
        {
            soundPlayerFactory = factory;
        }

        public event Action NextTrackRequested;

        public double CrossfadeDurationSeconds { get; set; } = 10.0;

        public double StartSeconds { get; set; } = 5.0;

        public double EndOffsetSeconds { get; set; } = 5.0;

        public double Volume { get => volume; set => SetProperty(ref volume, value); }

        public bool IsPlaying => NowPlaying();

        public void Play(SoundPlaybackItem newItem, double initialVolume = 1)
        {
            var toActivePlayer = soundPlayerFactory.Create();
            toActivePlayer.PlaybackStopped += OnPlaybackStopped;

            toActivePlayer.Volume = (float)initialVolume;
            toActivePlayer.Play(newItem);
            activePlayers.Enqueue(toActivePlayer);
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

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            var p = activePlayers.Dequeue();
            p.PlaybackStopped -= OnPlaybackStopped;

            // 他に再生中のプレイヤーがある場合はクロスフェードの最中なので、次を要求する必要はない。
            if (!NowPlaying())
            {
                NextTrackRequested?.Invoke();
            }
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
                    singlePlayer.Volume = (float)Volume;
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

                // 音量の適用（マスター音量を基準にフェード）
                var targetVolume = (float)Volume;
                oldPlayer.Volume = targetVolume * (1.0f - progress); // Volume -> 0.0 へ減少
                newPlayer.Volume = targetVolume * progress; // 0.0 -> Volume へ
            }
        }
    }
}