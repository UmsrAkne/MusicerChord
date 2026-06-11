using MusicerChord.Core;
using MusicerChord.Models;

namespace MusicerChord.Tests.Core
{
    [TestFixture]
    public class SoundPlaylistTests
    {
        private SoundPlaybackItem CreateItem(string path, bool isSkipped = false)
        {
            return new SoundPlaybackItem(new SoundFile { RelativePath = path, IsSkip = isSkipped, });
        }

        [Test]
        public void Constructor_WithEmptyList_ShouldHaveNoItems()
        {
            // Arrange
            var playlist = new SoundPlaylist(Enumerable.Empty<SoundPlaybackItem>());

            // Assert
            Assert.IsFalse(playlist.HasItems);
        }

        [Test]
        public void Constructor_WithItems_ShouldHaveItems()
        {
            // Arrange
            var items = new List<SoundPlaybackItem> { CreateItem("test.mp3"), };
            var playlist = new SoundPlaylist(items);

            // Assert
            Assert.IsTrue(playlist.HasItems);
        }

        [Test]
        public void PeekNext_WithEmptyList_ShouldReturnNull()
        {
            // Arrange
            var playlist = new SoundPlaylist(Enumerable.Empty<SoundPlaybackItem>());

            // Act
            var next = playlist.PeekNext();

            // Assert
            Assert.IsNull(next);
        }

        [Test]
        public void PeekNext_ShouldReturnNextItemWithoutAdvancingIndex()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3");
            var items = new List<SoundPlaybackItem> { item1, item2, };
            var playlist = new SoundPlaylist(items);

            // Act & Assert
            // 初期状態は currentIndex = -1 なので、次は index 0
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1)); // 進んでいないことの確認

            playlist.MoveNext(); // index 0 に進める
            Assert.That(playlist.PeekNext(), Is.EqualTo(item2));
        }

        [Test]
        public void PeekNext_SkipTest_two_items()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3", true);
            var items = new List<SoundPlaybackItem> { item1, item2, };
            var playlist = new SoundPlaylist(items);

            // Act & Assert
            // 初期状態は currentIndex = -1 なので、次は index 0
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));

            playlist.MoveNext(); // index 0 に進める
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));
        }

        [Test]
        public void PeekNext_SkipTest_three_items()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3", true);
            var item3 = CreateItem("3.mp3");
            var items = new List<SoundPlaybackItem> { item1, item2, item3, };
            var playlist = new SoundPlaylist(items);

            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));

            playlist.MoveNext();
            Assert.That(playlist.PeekNext(), Is.EqualTo(item3));

            playlist.MoveNext();
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));
        }

        [Test]
        public void PeekNext_SkipTest_three_items_skip_last_item()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3");
            var item3 = CreateItem("3.mp3", true);
            var items = new List<SoundPlaybackItem> { item1, item2, item3, };
            var playlist = new SoundPlaylist(items);

            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));

            playlist.MoveNext();
            Assert.That(playlist.PeekNext(), Is.EqualTo(item2));

            playlist.MoveNext();
            Assert.That(playlist.PeekNext(), Is.EqualTo(item1));
        }

        [Test]
        public void MoveNext_WithEmptyList_ShouldReturnNull()
        {
            // Arrange
            var playlist = new SoundPlaylist(Enumerable.Empty<SoundPlaybackItem>());

            // Act
            var result = playlist.MoveNext();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void MoveNext_ShouldAdvanceIndexAndLoop()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3");
            var items = new List<SoundPlaybackItem> { item1, item2, };
            var playlist = new SoundPlaylist(items);

            // Act & Assert
            Assert.That(playlist.MoveNext(), Is.EqualTo(item1));
            Assert.That(playlist.MoveNext(), Is.EqualTo(item2));
            Assert.That(playlist.MoveNext(), Is.EqualTo(item1)); // ループ
        }

        [Test]
        public void ResetToFirst_WithEmptyList_ShouldReturnNull()
        {
            // Arrange
            var playlist = new SoundPlaylist(Enumerable.Empty<SoundPlaybackItem>());

            // Act
            var result = playlist.ResetToFirst();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ResetToFirst_ShouldSetIndexToZero()
        {
            // Arrange
            var item1 = CreateItem("1.mp3");
            var item2 = CreateItem("2.mp3");
            var items = new List<SoundPlaybackItem> { item1, item2, };
            var playlist = new SoundPlaylist(items);

            // Act
            playlist.MoveNext(); // item1
            playlist.MoveNext(); // item2
            var result = playlist.ResetToFirst();

            // Assert
            Assert.That(result, Is.EqualTo(item1));
            Assert.That(playlist.PeekNext(), Is.EqualTo(item2));
        }
    }
}