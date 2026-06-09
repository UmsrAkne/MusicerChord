using System.Collections.Generic;
using System.Threading.Tasks;
using MusicerChord.Models;

namespace MusicerChord.Databases
{
    public interface IListenHistoryRepository
    {
        Task<IEnumerable<ListenHistory>> GetAllAsync();

        Task<ListenHistory?> GetByIdAsync(int id);

        Task AddAsync(ListenHistory listenHistory);

        Task UpdateAsync(ListenHistory listenHistory);

        Task DeleteAsync(int id);
    }
}