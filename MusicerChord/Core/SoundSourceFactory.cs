using System.Collections.Generic;
using System.IO;
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
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), absolutePath);

            // 1. ディレクトリの場合
            if (Directory.Exists(absolutePath))
            {
                // 自身をDirectorySoundSourceとして追加
                result.Add(new DirectorySoundSource(relativePath, absolutePath));

                // 中にあるファイルやディレクトリを再帰的、あるいはフラットに走査して変換
                foreach (var subPath in Directory.GetFileSystemEntries(absolutePath))
                {
                    result.AddRange(CreateFromPath(subPath)); // 再帰的に処理
                }
            }

            // 2. ファイルの場合
            else if (File.Exists(absolutePath))
            {
                var extension = Path.GetExtension(absolutePath).ToLower();

                if (extension is ".m3u" or ".m3u8")
                {
                    result.Add(new M3USoundSource(relativePath, absolutePath));
                }
            }

            return result;
        }
    }
}