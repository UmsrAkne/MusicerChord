using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicerChord.Models;

namespace MusicerChord.Databases
{
    public class ListenHistoryRepository : IListenHistoryRepository
    {
        private readonly MyDbContext context;

        public ListenHistoryRepository(MyDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ListenHistory>> GetAllAsync()
        {
            return await context.ListenHistories.ToListAsync();
        }

        public async Task<ListenHistory> GetByIdAsync(int id)
        {
            return await context.ListenHistories.FindAsync(id);
        }

        public async Task AddAsync(ListenHistory listenHistory)
        {
            await context.ListenHistories.AddAsync(listenHistory);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ListenHistory listenHistory)
        {
            context.ListenHistories.Update(listenHistory);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var listenHistory = await context.ListenHistories.FindAsync(id);
            if (listenHistory != null)
            {
                context.ListenHistories.Remove(listenHistory);
                await context.SaveChangesAsync();
            }
        }
    }
}