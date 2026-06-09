using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicerChord.Models;

namespace MusicerChord.Databases
{
    public class SoundFileService
    {
        private readonly ISoundFileRepository soundFileRepository;
        private readonly IListenHistoryRepository listenHistoryRepository;

        public SoundFileService(ISoundFileRepository soundFileRepository, IListenHistoryRepository listenHistoryRepository)
        {
            this.soundFileRepository = soundFileRepository;
            this.listenHistoryRepository = listenHistoryRepository;
        }

        public async Task<IEnumerable<SoundFile>> GetAllSoundFilesAsync()
        {
            return await soundFileRepository.GetAllAsync();
        }

        public async Task RecordListenHistoryAsync(int soundFileId)
        {
            var history = new ListenHistory
            {
                SoundFileId = soundFileId,
                ListenedAt = DateTime.Now,
            };
            await listenHistoryRepository.AddAsync(history);
        }

        // その他、ビジネスロジックに応じたメソッドをここに追加
    }
}