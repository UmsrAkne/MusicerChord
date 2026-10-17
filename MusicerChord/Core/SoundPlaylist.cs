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

        /// <summary>
        /// 現在の再生位置を指定したアイテムのインデックスへ安全に変更します。
        /// </summary>
        /// <param name="item">設定したいアイテム。</param>
        /// <param name="allowSkippedItem">スキップ対象(IsSkip == true)のアイテムへの変更を許可するかどうか。</param>
        /// <returns>変更に成功した場合はセットしたアイテムに対応する SoundPlaybackItem を返します。セットできなかった場合は null を返します</returns>
        public SoundPlaybackItem TrySetCurrentItem(SoundFile item, bool allowSkippedItem = false)
        {
            if (item == null)
            {
                return null;
            }

            var targetItem = items.FirstOrDefault(i => i.SoundFile.FullPath == item.FullPath);
            if (targetItem == null)
            {
                return null;
            }

            if (TrySetIndex(items.IndexOf(targetItem), allowSkippedItem))
            {
                return targetItem;
            }

            return null;
        }

        /// <summary>
        /// 指定したインデックスに現在の再生位置を安全に変更します。
        /// </summary>
        /// <param name="index">設定したいインデックス。</param>
        /// <param name="allowSkippedItem">スキップ対象(IsSkip == true)のアイテムへの変更を許可するかどうか。</param>
        /// <returns>変更に成功した場合は true、インデックスが範囲外またはスキップ対象の場合は false。</returns>
        private bool TrySetIndex(int index, bool allowSkippedItem = false)
        {
            // リストの範囲外チェック
            if (index < 0 || index >= items.Count)
            {
                return false;
            }

            // スキップ対象アイテムのチェック
            if (!allowSkippedItem && items[index].SoundFile.IsSkip)
            {
                return false;
            }

            currentIndex = index;
            return true;
        }

        private SoundPlaybackItem GetNextItem()
        {
            var nextItem = items.Skip(currentIndex + 1).FirstOrDefault(s => !s.SoundFile.IsSkip);
            return nextItem ?? items.FirstOrDefault(s => !s.SoundFile.IsSkip);
        }
    }
}