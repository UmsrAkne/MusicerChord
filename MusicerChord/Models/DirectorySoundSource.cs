using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MusicerChord.Core;
using Prism.Mvvm;

namespace MusicerChord.Models
{
    public class DirectorySoundSource : BindableBase, ISoundContainer
    {
        private readonly string absoluteRootPath; // アプリ設定のルートパス
        private ObservableCollection<ISoundContainer> children = new ();
        private bool hasChildren;
        private AsyncRelayCommand loadChildrenCommand;

        public DirectorySoundSource(string relativePath, string absoluteRootPath)
        {
            Path = relativePath;
            this.absoluteRootPath = absoluteRootPath;
            AbsolutePath = absoluteRootPath;
        }

        public string Name => System.IO.Path.GetFileName(Path);

        public string Path { get; } // 例: "Ambient/Nature"

        public bool HasChildren { get => hasChildren; set => SetProperty(ref hasChildren, value); }

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

        public string AbsolutePath { get; set; }

        public AsyncRelayCommand LoadChildrenCommand =>
        loadChildrenCommand ??= new AsyncRelayCommand(async () =>
        {
            if (!HasChildren)
            {
                return;
            }

            var items = await Task.Run(() => SoundSourceFactory.CreateFromPath(AbsolutePath));
            Children.Clear();
            Children.AddRange(items);
        });

        public IEnumerable<string> GetRelativeFilePaths()
        {
            // 実際のフルパスを組み立てて、そこからファイルをスキャンする
            var fullPath = System.IO.Path.Combine(absoluteRootPath, Path);

            if (!Directory.Exists(fullPath))
            {
                return Enumerable.Empty<string>();
            }

            // フォルダ内の音声ファイルを列挙し、相対パスに変換して返す
            return Directory.EnumerateFiles(fullPath, "*.*", SearchOption.TopDirectoryOnly)
                .Select(fp => System.IO.Path.GetRelativePath(absoluteRootPath, fp));
        }
    }
}