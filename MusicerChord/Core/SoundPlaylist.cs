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

        /// <summary>
        /// インデックスを変更せず、次のアイテムを取得します。
        /// </summary>
        /// <returns>次のインデックスにあたるアイテム。</returns>
        public SoundPlaybackItem PeekNext()
        {
            if (!items.Any())
            {
                return null;
            }

            var nextIndex = currentIndex >= items.Count - 1 ? 0 : currentIndex + 1;
            return items[nextIndex];
        }

        /// <summary>
        /// 次のアイテムを取得し、インデックスを進めます。
        /// </summary>
        /// <returns>次のインデックスにあたるアイテム。</returns>
        public SoundPlaybackItem MoveNext()
        {
            if (!items.Any())
            {
                return null;
            }

            currentIndex = currentIndex >= items.Count - 1 ? 0 : currentIndex + 1;
            return items[currentIndex];
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

            currentIndex = 0;
            return items[currentIndex];
        }
    }
}