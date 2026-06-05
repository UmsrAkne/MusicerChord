using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicerChord.Models;

namespace MusicerChord.Core
{
    public static class SoundSourceFactory
    {
        /// <summary>
        /// 指定されたパス（ファイルまたはディレクトリ）を解析し、適切なSoundSourceのリストを返します。
        /// </summary>
        /// <param name="absolutePath">取得したいアイテムが格納されているディレクトリの絶対パス</param>
        /// <returns>指定パスの中のアイテムを変換した ISoundContainer</returns>
        public static List<ISoundContainer> CreateFromPath(string absolutePath)
        {
            var result = new List<ISoundContainer>();

            // EnumerateFileSystemEntries を使うことで、配列を作らずに1階層目を列挙
            foreach (var subPath in Directory.EnumerateFileSystemEntries(absolutePath))
            {
                if (Directory.Exists(subPath))
                {
                    var relative = Path.GetRelativePath(absolutePath, subPath);
                    var hasChildren = Directory.EnumerateFileSystemEntries(subPath).Any(childPath =>
                    {
                        if (Directory.Exists(childPath))
                        {
                            return true; // ディレクトリなら即座に true
                        }

                        return IsM3UFile(childPath); // ファイルなら拡張子をチェック
                    });

                    // コンストラクタやプロパティで hasChildren を渡す
                    var item = new DirectorySoundSource(relative, subPath)
                    {
                        HasChildren = hasChildren,
                    };

                    if (hasChildren)
                    {
                        // 表示のためにダミーの要素を入れる
                        item.Children.Add(new DirectorySoundSource(string.Empty, string.Empty));
                    }

                    result.Add(item);
                }

                if (File.Exists(subPath))
                {
                    if (IsM3UFile(subPath))
                    {
                        var rel = Path.GetRelativePath(subPath, absolutePath);
                        result.Add(new M3USoundSource(rel, absolutePath));
                    }
                }
            }

            return result;
        }

        private static bool IsM3UFile(string subPath)
        {
            var extension = Path.GetExtension(subPath);
            return extension.Equals(".m3u", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
        }
    }
}