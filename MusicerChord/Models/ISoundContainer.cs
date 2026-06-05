using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace MusicerChord.Models
{
    public interface ISoundContainer
    {
        // アプリの画面（ツリービューやリストなど）に表示する名前
        // 例: "お気に入りフォルダ" や "作業用BGM.m3u"
        string Name { get; }

        // ルートディレクトリからの相対パス（ソース自体の識別子）
        // 例: "Playlists/anime.m3u" や "Spcial/BattleBGM"
        string Path { get; }

        ObservableCollection<ISoundContainer> Children { get; }

        string AbsolutePath { get; set; }

        static bool HasChildren { get; set; }

        AsyncRelayCommand LoadChildrenCommand { get; }

        // このソースが内包しているサウンドの相対パス一覧を返す
        // 後々DB化や遅延読み込み（IAsyncEnumerableなど）にする際も、このシグネチャなら対応しやすいです
        IEnumerable<string> GetRelativeFilePaths();
    }
}