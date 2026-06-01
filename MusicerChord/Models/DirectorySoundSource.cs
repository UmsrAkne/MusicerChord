using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class DirectorySoundSource : BindableBase, ISoundContainer
    {
        private readonly string absoluteRootPath; // アプリ設定のルートパス
        private ObservableCollection<ISoundContainer> children;

        public DirectorySoundSource(string relativePath, string absoluteRootPath)
        {
            Path = relativePath;
            this.absoluteRootPath = absoluteRootPath;
        }

        public string Name => System.IO.Path.GetFileName(Path);

        public string Path { get; } // 例: "Ambient/Nature"

        public ObservableCollection<ISoundContainer> Children
        {
            get => children;
            set
            {
                if (Equals(value, children))
                {
                    return;
                }

                children = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> GetRelativeFilePaths()
        {
            // 実際のフルパスを組み立てて、そこからファイルをスキャンする
            var fullPath = System.IO.Path.Combine(absoluteRootPath, Path);

            if (!System.IO.Directory.Exists(fullPath))
            {
                return Enumerable.Empty<string>();
            }

            // フォルダ内の音声ファイルを列挙し、相対パスに変換して返す
            return System.IO.Directory.EnumerateFiles(fullPath, "*.*", System.IO.SearchOption.TopDirectoryOnly)
                // .mp3 や .wav などでフィルタリング
                .Select(fp => System.IO.Path.GetRelativePath(absoluteRootPath, fp));
        }
    }
}