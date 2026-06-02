using System;
using System.IO;
using System.Text.Json;

namespace MusicerChord.Utils
{
    public class AppSettings
    {
        public readonly static string SettingFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        /// <summary>
        /// アプリケーションが取り扱うルート（基準となるディレクトリ）の絶対パス。
        /// </summary>
        public string RootPath { get; set; } = string.Empty;

        public static AppSettings Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var newAppSettings = new AppSettings();
                newAppSettings.Save(filePath);
                return newAppSettings;
            }

            var jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppSettings>(jsonString) ?? new AppSettings();
        }

        public void Save(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, };
            var jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, jsonString);
        }
    }
}