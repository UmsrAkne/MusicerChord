using System.Windows.Threading;
using Moq;
using MusicerChord.Core;
using MusicerChord.Databases;
using MusicerChord.Models;

namespace MusicerChord.Tests.Core
{
    [TestFixture]
    public class SoundPlayerServiceTests
    {
        private SoundPlaybackItem Create(string fileName)
        {
            return new SoundPlaybackItem(new SoundFile { RelativePath = $"path/to/{fileName}", });
        }

        [Test]
        public void PlayNext_WhenCanExecuteCrossfade_ShouldPlayWithZeroVolumeAndAdvancePlaylist()
        {
            // 1. 準備 (Arrange)
            var mockController = new Mock<ICrossfadeController>();
            var mockFileService = new Mock<SoundFileService>(new Mock<ISoundFileRepository>().Object,
                new Mock<IListenHistoryRepository>().Object);

            var dtProvider = new Mock<IDateTimeProvider>();
            var timer = new DispatcherTimer();

            var item1 = Create("sound1.mp3");
            var item2 = Create("sound2.mp3");
            var list = new List<SoundPlaybackItem> { item1, item2, };

            // プレイリストのモック（またはテスト用の軽いダミーオブジェクト）
            var playlist = new SoundPlaylist(list);

            var service =
                new SoundPlayerService(mockFileService.Object, mockController.Object, dtProvider.Object, timer)
                {
                    SoundPlaylist = playlist,
                };

            // コントローラーの振る舞いを設定
            mockController.Setup(c => c.IsPlaying).Returns(true);
            mockController.Setup(c => c.CanExecuteCrossfade(It.IsAny<SoundPlaybackItem>())).Returns(true);

            // 一曲目を再生開始
            service.Play();

            // 2. 実行 (Act)
            // 一曲目の再生中にイベントが出る状態を再現。強制的に発火させて PlayNext を走らせる
            mockController.Raise(c => c.NextTrackRequested += null);

            // 3. 検証 (Assert)
            // ボリューム 0 で次の曲が再生されたか？（渡された SoundFile の名前は想定通りか？）
            mockController.Verify(
                c => c.Play(It.Is<SoundPlaybackItem>(item => item.SoundFile.FileName == item2.SoundFile.FileName), 0),
                Times.Once);

            // プレイリストのインデックスが進んでいるか？
            // (MoveNextが呼ばれた結果、現在のアイテムやインデックスが期待通りか検証)
            Assert.That(playlist.CurrentItem, Is.EqualTo(item2));
        }
    }
}