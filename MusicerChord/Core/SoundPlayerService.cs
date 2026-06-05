using System;

namespace MusicerChord.Core
{
    /// <summary>
    /// SoundListViewModel に配置され、音声の再生制御ロジックを提供するサービスクラスです。
    /// </summary>
    public class SoundPlayerService
    {
        public void Play()
        {
            Console.WriteLine("Play");
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}