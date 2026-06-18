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
        private DateTime lastLoadedTime;
        private int depth;
        private bool hasSubDirectory;
        private bool hasSoundFile;

        public DirectorySoundSource(string relativePath, string absoluteRootPath)
        {
            Path = relativePath;
            this.absoluteRootPath = absoluteRootPath;
            AbsolutePath = absoluteRootPath;
        }

        public Func<ISoundContainer, IEnumerable<ISoundContainer>, Task> RequestInsertChildren { get; set; }

        public string Name => System.IO.Path.GetFileName(Path);

        public string Path { get; } // 例: "Ambient/Nature"

        public bool HasChildren { get => hasChildren; set => SetProperty(ref hasChildren, value); }

        public bool HasSubDirectory { get => hasSubDirectory; set => SetProperty(ref hasSubDirectory, value); }

        public bool HasSoundFile { get => hasSoundFile; set => SetProperty(ref hasSoundFile, value); }

        public bool IsEmpty => !HasChildren && !HasSoundFile;

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

        public int Depth { get => depth; set => SetProperty(ref depth, value); }

        public AsyncRelayCommand LoadChildrenCommand =>
            loadChildrenCommand ??= new AsyncRelayCommand(async () =>
            {
                if (!HasChildren)
                {
                    return;
                }

                var currentWriteTime = GetSourceLastWriteTime(AbsolutePath);

                // 前回読み込んだ時と日時が同じなら、中身が変わっていないので中断。
                if (lastLoadedTime == currentWriteTime)
                {
                    return;
                }

                var items = await Task.Run(() => SoundSourceFactory.CreateFromPath(AbsolutePath));
                foreach (var soundContainer in items)
                {
                    soundContainer.Depth = Depth + 1;
                }

                RequestInsertChildren?.Invoke(this, items);

                lastLoadedTime = currentWriteTime;
            });

        public void UpdateDirectoryStatus(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            {
                HasSubDirectory = false;
                HasSoundFile = false;
                return;
            }

            try
            {
                // サブディレクトリが1つでも存在するかチェック
                HasSubDirectory = Directory.EnumerateDirectories(targetPath).Any();

                // .mp3 ファイルが1つでも存在するかチェック
                var enumerationOptions = new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = false, // 今回の階層直下のみ
                };

                HasSoundFile = Directory.EnumerateFiles(targetPath, "*.mp3", enumerationOptions).Any();
            }
            catch (Exception e)
            {
                HasSubDirectory = false;
                HasSoundFile = false;
                Console.WriteLine(e.Message);
            }
        }

        public async Task<IEnumerable<ISoundContainer>> LoadChildren()
        {
            if (!HasChildren)
            {
                return new List<ISoundContainer>();
            }

            var currentWriteTime = GetSourceLastWriteTime(AbsolutePath);

            // 前回読み込んだ時と日時が同じなら、中身が変わっていないので中断。
            if (lastLoadedTime == currentWriteTime)
            {
                return Children.ToList();
            }

            var items = await Task.Run(() => SoundSourceFactory.CreateFromPath(AbsolutePath));
            foreach (var soundContainer in items)
            {
                soundContainer.Depth = Depth + 1;
            }

            Children.Clear();
            Children.AddRange(items);
            lastLoadedTime = currentWriteTime;
            return items;
        }

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

        private DateTime GetSourceLastWriteTime(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetLastWriteTime(path);
            }

            return File.Exists(path)
                ? File.GetLastWriteTime(path)
                : DateTime.MinValue;
        }
    }
}