using System.Collections.Generic;
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
    }
}