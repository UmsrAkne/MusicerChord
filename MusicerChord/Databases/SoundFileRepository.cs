using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicerChord.Models;

namespace MusicerChord.Databases
{
    public class SoundFileRepository : ISoundFileRepository
    {
        private readonly MyDbContext context;

        public SoundFileRepository(MyDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SoundFile>> GetAllAsync()
        {
            return await context.SoundFiles.ToListAsync();
        }

        public async Task<SoundFile> GetByIdAsync(int id)
        {
            return await context.SoundFiles.FindAsync(id);
        }

        public async Task AddAsync(SoundFile soundFile)
        {
            await context.SoundFiles.AddAsync(soundFile);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SoundFile soundFile)
        {
            context.SoundFiles.Update(soundFile);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var soundFile = await context.SoundFiles.FindAsync(id);
            if (soundFile != null)
            {
                context.SoundFiles.Remove(soundFile);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SoundFile>> GetByRelativePathsAsync(IEnumerable<string> relativePaths)
        {
            if (relativePaths == null)
            {
                return Enumerable.Empty<SoundFile>();
            }

            var pathList = relativePaths.ToArray();
            if (pathList.Length == 0)
            {
                return Enumerable.Empty<SoundFile>();
            }

            return await context.SoundFiles
                .Where(f => pathList.Contains(f.RelativePath))
                .ToListAsync();
        }

        public async Task SaveRangeAsync(IEnumerable<SoundFile> soundFiles)
        {
            var files = soundFiles.ToList();
            if (!files.Any())
            {
                return;
            }

            foreach (var file in files)
            {
                // IDが0なら新規追加、それ以外は更新状態にする
                if (file.Id == 0)
                {
                    await context.SoundFiles.AddAsync(file);
                }
                else
                {
                    context.SoundFiles.Update(file);
                }
            }

            // 最後に1回だけDBに問い合わせる（劇的に速くなります）
            await context.SaveChangesAsync();
        }
    }
}