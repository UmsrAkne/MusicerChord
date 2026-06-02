using MusicerChord.Models;

namespace MusicerChord.Core
{
    public class SoundPathResolver
    {
        private readonly string currentRootPath;

        public SoundPathResolver(string currentRootPath)
        {
            // アプリの設定などから「D:\Sound」や「E:\Audio」などの最新のルートを受け取る
            this.currentRootPath = currentRootPath;
        }

        public string ResolveFullPath(SoundFile soundFile)
        {
            // Path.Combine を使えば、OSの区切り文字（\ や /）を安全に結合できます
            return System.IO.Path.Combine(currentRootPath, soundFile.RelativePath);
        }
    }
}