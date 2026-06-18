using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;

namespace MusicerChord.Models;

public class M3USoundSource : BindableBase, ISoundContainer
{
    private readonly string absoluteRootPath;
    private bool hasChildren;

    public M3USoundSource(string relativePath, string absoluteRootPath)
    {
        Path = relativePath;
        this.absoluteRootPath = absoluteRootPath;
        AbsolutePath = absoluteRootPath;
    }

    public Func<ISoundContainer, IEnumerable<ISoundContainer>, Task> RequestInsertChildren { get; set; }

    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    public string Path { get; } // 例: "Playlists/my_favorite.m3u"

    public ObservableCollection<ISoundContainer> Children { get; } = new ();

    public string AbsolutePath { get; set; }

    public bool HasSubDirectory { get; set; }

    public bool HasChildren { get => hasChildren; set => SetProperty(ref hasChildren, value); }

    public bool HasSoundFile { get; set; }

    public bool IsEmpty => !HasChildren && !HasSoundFile;

    public int Depth { get; set; }

    public AsyncRelayCommand LoadChildrenCommand { get; } = new (() => Task.CompletedTask);

    public Task<IEnumerable<ISoundContainer>> LoadChildren()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetRelativeFilePaths()
    {
        var fullPath = System.IO.Path.Combine(absoluteRootPath, Path);

        if (!System.IO.File.Exists(fullPath))
        {
            return Enumerable.Empty<string>();
        }

        // M3Uファイルをテキストとして読み込み、中身（相対パスのリスト）を解析して返す
        var paths = new List<string>();
        foreach (var line in System.IO.File.ReadLines(fullPath))
        {
            // コメント行（先頭#）や空行をスキップ
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                continue;
            }

            // M3U内に書かれているパスを、ルートからの相対パスに補正して追加
            paths.Add(line.Trim());
        }

        return paths;
    }

    public void UpdateDirectoryStatus(string targetPath)
    {
        throw new NotImplementedException();
    }
}