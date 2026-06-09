using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MusicerChord.Models
{
    [Index(nameof(SoundFileId))] // インデックスを追加
    public class ListenHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SoundFileId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ListenedAt { get; set; }
    }
}