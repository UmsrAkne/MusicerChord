using System;
using System.IO;

namespace MusicerChord.Core
{
    public class AudioMetadataExtractor : IMetadataReader
    {
        /// <summary>
        /// ファイルの絶対パスからメタデータの「タイトル」を取得します。
        /// 読み取り不可、または格納されていない場合は空文字を返します。
        /// </summary>
        /// <param name="absolutePath">ファイルの絶対パス</param>
        /// <returns>メタデータのタイトル（存在しない場合は空文字）</returns>
        public string GetFileTitle(string absolutePath)
        {
            // 事前チェック: ファイルが存在しない場合は空文字を返す
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
            {
                return string.Empty;
            }

            try
            {
                using var file = TagLib.File.Create(absolutePath);

                // タグ情報が存在し、かつタイトルが格納されているかチェック
                if (file.Tag != null && !string.IsNullOrWhiteSpace(file.Tag.Title))
                {
                    return file.Tag.Title;
                }
            }
            catch (Exception)
            {
                // 読み取り不可能の場合
                return string.Empty;
            }

            // メタデータにタイトルが格納されていなかった場合
            return string.Empty;
        }
    }
}