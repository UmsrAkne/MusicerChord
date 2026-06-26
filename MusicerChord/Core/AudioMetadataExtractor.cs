using System;
using System.IO;
using System.Text.RegularExpressions;

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
                    var title = file.Tag.Title;

                    if (IsGarbled(title))
                    {
                        // やばい文字列なら、諦めてファイル名を採用（UIの防衛）
                        return Path.GetFileNameWithoutExtension(absolutePath);
                    }

                    return title;
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

        private bool IsGarbled(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            // 判定1: 「?」の比率が多すぎる
            var questionCount = text.Length - text.Replace("?", string.Empty).Length;
            if (questionCount > 0)
            {
                var questionRatio = (double)questionCount / text.Length;
                if (questionRatio >= 0.2 || text.Contains("???"))
                {
                    return true;
                }
            }

            // 判定2: ヨーロッパ系アクセント文字の異常な密集
            // 日本語のタイトルに、フランス語やドイツ語などの特殊文字（À-ÿ）が3文字以上含まれていれば異常判定。
            var latinAccentCount = Regex.Matches(text, @"[À-ÿ]").Count;
            if (latinAccentCount >= 3)
            {
                return true;
            }

            // --- 判定3: 制御文字の混入 ---
            foreach (var c in text)
            {
                if (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t')
                {
                    return true;
                }
            }

            return false;
        }
    }
}