using System.Collections.Generic;
using System.Threading.Tasks;
using MusicerChord.Models;

namespace MusicerChord.Databases
{
    public interface ISoundFileRepository
    {
        Task<IEnumerable<SoundFile>> GetAllAsync();

        Task<SoundFile?> GetByIdAsync(int id);

        Task AddAsync(SoundFile soundFile);

        Task UpdateAsync(SoundFile soundFile);

        Task DeleteAsync(int id);
    }
}