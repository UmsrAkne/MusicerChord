using System;
using System.IO;
using MusicerChord.Models;
using NAudio.Wave;

namespace MusicerChord.Core
{
    public class SoundPlayer : IDisposable, ISoundPlayer
    {
        private AudioFileReader audioFile;
        private WaveOutEvent outputDevice;
        private SoundPlaybackItem currentItem;
        private bool isDisposed;
        private float volume = 1.0f;

        /// <summary>
        /// 音量（0.0 ～ 1.0）を取得または設定します。
        /// </summary>
        public float Volume
        {
            get => volume;
            set
            {
                volume = Math.Clamp(value, 0.0f, 1.0f);
                if (audioFile != null)
                {
                    audioFile.Volume = volume;
                }
            }
        }

        /// <summary>
        /// 対象のアイテムを読み込んで再生します。既に再生中の場合は停止して切り替えます。
        /// </summary>
        public void Play(SoundPlaybackItem item, double startSeconds = 0)
        {
            if (item?.SoundFile?.FullPath == null || !File.Exists(item.SoundFile.FullPath))
            {
                return;
            }

            // 一時停止からの再開ならそのまま再生
            if (currentItem == item && outputDevice?.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                UpdateItemState();
                return;
            }

            StopAndRelease();
            currentItem = item;

            try
            {
                audioFile = new AudioFileReader(currentItem.SoundFile.FullPath);
                audioFile.Volume = volume;

                if (startSeconds > 0)
                {
                    audioFile.CurrentTime = TimeSpan.FromSeconds(startSeconds);
                }

                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;

                outputDevice.Init(audioFile);
                outputDevice.Play();

                UpdateItemState();
            }
            catch (Exception)
            {
                StopAndRelease();
                throw;
            }
        }

        /// <summary>
        /// 再生を一時停止します。
        /// </summary>
        public void Pause()
        {
            if (outputDevice?.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                UpdateItemState();
            }
        }

        /// <summary>
        /// 再生を完全に停止します。
        /// </summary>
        public void Stop()
        {
            StopAndRelease();
        }

        /// <summary>
        /// 現在の再生位置を強制的に変更（シーク）します。
        /// </summary>
        public void SetPlaybackTime(TimeSpan time)
        {
            if (audioFile == null)
            {
                return;
            }

            // 再生時間をファイルの総時間内に収めるガード
            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }

            if (time > audioFile.TotalTime)
            {
                time = audioFile.TotalTime;
            }

            audioFile.CurrentTime = time;

            if (currentItem != null)
            {
                // 注意: CurrentPlaybackTime に変更通知（RaisePropertyChanged等）がない場合は、
                // ViewModel側で別途通知を呼ぶ必要があります。
                currentItem.CurrentPlaybackTime = time;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// SoundPlaybackItem のプロパティを現在の NAudio の状態と同期します。
        /// </summary>
        public void UpdateItemState()
        {
            if (currentItem == null)
            {
                return;
            }

            var currentState = outputDevice?.PlaybackState ?? PlaybackState.Stopped;

            currentItem.PlaybackState = currentState;
            currentItem.IsPlaying = currentState == PlaybackState.Playing;
            currentItem.CurrentPlaybackTime = audioFile.CurrentTime;
        }

        /// <summary>
        /// 内部リソースの停止と解放。
        /// </summary>
        public void StopAndRelease()
        {
            if (outputDevice != null)
            {
                outputDevice.PlaybackStopped -= OnPlaybackStopped;
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            if (currentItem != null)
            {
                // 参照を外す前に最後の状態を同期
                currentItem.PlaybackState = PlaybackState.Stopped;
                currentItem.IsPlaying = false;
                currentItem = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    StopAndRelease();
                }

                isDisposed = true;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // UIスレッドで状態更新を安全に行う
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                UpdateItemState();

                // 自然に再生が終了した場合はリソースを逃がす
                if (outputDevice?.PlaybackState == PlaybackState.Stopped)
                {
                    StopAndRelease();
                }
            });
        }
    }
}