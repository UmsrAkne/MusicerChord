using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    }

    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

    public string Path { get; } // 例: "Playlists/my_favorite.m3u"

    public ObservableCollection<ISoundContainer> Children { get; } = new ();

    public bool HasChildren { get => hasChildren; set => SetProperty(ref hasChildren, value); }

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
}