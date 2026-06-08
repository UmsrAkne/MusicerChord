using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MusicerChord.Models;
using MusicerChord.Utils;

namespace MusicerChord.Databases
{
    public class MyDbContext : DbContext
    {
        public DbSet<SoundFile> SoundFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var baseDir = AppContext.BaseDirectory;
                var dbPath = Path.Combine(baseDir, "listen_history.db");
                Logger.Log($"DB Path: {dbPath}");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}