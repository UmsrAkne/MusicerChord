using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    [Index(nameof(RelativePath), IsUnique = true)]
    public class SoundFile : BindableBase
    {
        private int durationMs;
        private int playCount;
        private int lineNumber;
        private bool isSkip;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(1024)]
        public string RelativePath { get; set; } // 例: "Ambient/01_rain.mp3" (ルートが D:\Sound なら、実際は D:\Sound\Ambient\01_rain.mp3)

        [NotMapped]
        public string FileName => Path.GetFileName(RelativePath);

        [NotMapped]
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(RelativePath);

        [Required]
        public int DurationMs
        {
            get => durationMs;
            set
            {
                if (SetProperty(ref durationMs, value))
                {
                    RaisePropertyChanged(nameof(DurationText));
                }
            }
        }

        [NotMapped]
        public string DurationText => TimeSpan.FromMilliseconds(DurationMs).ToString(@"hh\:mm\:ss");

        [Required]
        public bool IsSkip { get => isSkip; set => SetProperty(ref isSkip, value); }

        [NotMapped]
        public int PlayCount { get => playCount; set => SetProperty(ref playCount, value); }

        [NotMapped]
        public string FullPath { get; set; }

        [NotMapped]
        public int LineNumber { get => lineNumber; set => SetProperty(ref lineNumber, value); }
    }
}