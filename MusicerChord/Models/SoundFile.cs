using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class SoundFile : BindableBase
    {
        private int durationMs;

        public int Id { get; set; }

        // 例: "Ambient/01_rain.mp3" (ルートが D:\Sound なら、実際は D:\Sound\Ambient\01_rain.mp3)
        public string RelativePath { get; set; }

        public string FileName => Path.GetFileName(RelativePath);

        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(RelativePath);

        public int DurationMs { get => durationMs; set => SetProperty(ref durationMs, value); }

        public bool IsSkip { get; set; }

        [NotMapped]
        public int PlayCount { get; set; }

        [NotMapped]
        public string FullPath { get; set; }
    }
}