using System.Collections.Generic;
using System.Linq;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public class SoundPlaylist
    {
        private readonly List<SoundPlaybackItem> items;
        private int currentIndex = -1;

        public SoundPlaylist(IEnumerable<SoundPlaybackItem> items)
        {
            this.items = items.ToList();
        }

        public bool HasItems => items.Any();

        public SoundPlaybackItem CurrentItem => items[currentIndex];

        /// <summary>
        /// インデックスを変更せず、次のアイテムを取得します。
        /// </summary>
        /// <returns>次のインデックスにあたるアイテム。</returns>
        public SoundPlaybackItem PeekNext()
        {
            return GetNextItem();
        }

        /// <summary>
        /// 次のアイテムを取得し、インデックスを進めます。
        /// </summary>
        /// <returns>次のインデックスにあたるアイテム。</returns>
        public SoundPlaybackItem MoveNext()
        {
            var item = GetNextItem();
            if (item != null)
            {
                currentIndex = items.IndexOf(item);
            }

            return item;
        }

        /// <summary>
        /// インデックスを 0 にリセットし、リストの先頭のアイテムを取得します。
        /// </summary>
        /// <returns>リストの先頭のアイテム。リストが空の場合は null。</returns>
        public SoundPlaybackItem ResetToFirst()
        {
            if (!items.Any())
            {
                return null;
            }

            var firstValidIndex = items.FindIndex(item => !item.SoundFile.IsSkip);
            if (firstValidIndex == -1)
            {
                return null;
            }

            currentIndex = firstValidIndex;
            return items[currentIndex];
        }

        private SoundPlaybackItem GetNextItem()
        {
            var nextItem = items.Skip(currentIndex + 1).FirstOrDefault(s => !s.SoundFile.IsSkip);
            return nextItem ?? items.FirstOrDefault(s => !s.SoundFile.IsSkip);
        }
    }
}