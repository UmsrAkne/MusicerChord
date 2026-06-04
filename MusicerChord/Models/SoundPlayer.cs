using System;
using System.IO;
using NAudio.Wave;

namespace MusicerChord.Models
{
    public class SoundPlayer : IDisposable
    {
        private AudioFileReader audioFile;
        private WaveOutEvent outputDevice;
        private SoundPlaybackItem currentItem;
        private bool isDisposed;

        /// <summary>
        /// 対象のアイテムを読み込んで再生します。既に再生中の場合は停止して切り替えます。
        /// </summary>
        public void Play(SoundPlaybackItem item)
        {
            if (item?.SoundFile?.FullPath == null || !File.Exists(item.SoundFile.FullPath))
            {
                throw new FileNotFoundException("再生対象のファイルが見つかりません。", item?.SoundFile?.FullPath);
            }

            // 同じアイテムで一時停止中だった場合は、再開処理を行う
            if (currentItem == item && outputDevice?.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                UpdateItemState();
                return;
            }

            // 別の曲を再生中なら一度クリーンアップ
            StopAndRelease();

            currentItem = item;

            try
            {
                audioFile = new AudioFileReader(currentItem.SoundFile.FullPath);
                outputDevice = new WaveOutEvent();

                // 再生終了イベントのハンドラを登録
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

        /// <summary>
        /// SoundPlaybackItem のプロパティを現在の NAudio の状態と同期します。
        /// </summary>
        private void UpdateItemState()
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
        private void StopAndRelease()
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
    }
}