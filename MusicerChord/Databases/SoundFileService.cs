using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MusicerChord.Models;
using NAudio.Wave;

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

        public async Task FillOrFetchDurationsAsync(IEnumerable<SoundFile> soundFiles)
        {
            if (soundFiles == null)
            {
                return;
            }

            var fileList = soundFiles.ToList();
            if (!fileList.Any())
            {
                return;
            }

            // 1. 引数から RelativePath の重複なきリストを作成
            var relativePaths = fileList.Select(f => f.RelativePath).Distinct().ToList();

            IEnumerable<SoundFile> dbFiles;

            // 2. DBから登録済みデータを RelativePath で一括検索して Dictionary 化
            dbFiles = await soundFileRepository.GetByRelativePathsAsync(relativePaths);

            var dbFileMap = dbFiles.ToDictionary(f => f.RelativePath, f => f);

            var filesToSaveToDb = new List<SoundFile>();

            // 3. マッチング処理
            foreach (var file in fileList)
            {
                // RelativePath でデータを引っ張る
                if (dbFileMap.TryGetValue(file.RelativePath, out var dbFile))
                {
                    // DBにデータがあった場合
                    file.Id = dbFile.Id;
                    file.DurationMs = dbFile.DurationMs;
                    file.IsSkip = dbFile.IsSkip; // ついでに他の永続化パラメーターも同期
                }
                else
                {
                    // DBになかった場合は、FullPath を使って実際のファイルを解析
                    var durationMs = LoadDurationFromFile(file.FullPath);
                    file.DurationMs = (int)durationMs;

                    // DB保存用としてマーク
                    filesToSaveToDb.Add(file);
                }
            }

            // 4. 新規解析データを一括保存
            if (filesToSaveToDb.Any())
            {
                await soundFileRepository.SaveRangeAsync(filesToSaveToDb);
            }
        }

        /// <summary>
        /// 実際の音声ファイルから再生時間をミリ秒単位で取得します（重いI/O処理）
        /// </summary>
        private double LoadDurationFromFile(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    return 0;
                }

                using var reader = new AudioFileReader(fullPath);
                return reader.TotalTime.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NAudioによる時間取得に失敗: {fullPath} - {ex.Message}");
                return 0;
            }
        }
    }
}