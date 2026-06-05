using Moq;
using MusicerChord.Core;
using MusicerChord.Models;

namespace MusicerChord.Tests.Core
{
    [TestFixture]
    public class CrossfadeControllerTests
    {
        private Mock<ISoundPlayer> _playerA;
        private Mock<ISoundPlayer> _playerB;
        private CrossfadeController _controller;

        private const double StartSeconds = 5.0;
        private const double EndOffsetSeconds = 5.0;
        private const double CrossfadeDurationSeconds = 10.0;
        private const double MinLongDurationMs = (StartSeconds + EndOffsetSeconds + CrossfadeDurationSeconds) * 1000;

        [SetUp]
        public void Setup()
        {
            _playerA = new Mock<ISoundPlayer>();
            _playerB = new Mock<ISoundPlayer>();
            _controller = new CrossfadeController(_playerA.Object, _playerB.Object)
            {
                StartSeconds = StartSeconds,
                EndOffsetSeconds = EndOffsetSeconds,
                CrossfadeDurationSeconds = CrossfadeDurationSeconds,
            };
        }

        private SoundPlaybackItem CreateItem(string name, int durationMs)
        {
            return new SoundPlaybackItem(new SoundFile { RelativePath = name, DurationMs = durationMs, });
        }

        private void SetupPlayer(Mock<ISoundPlayer> player, SoundPlaybackItem item, bool isPlaying, int totalTimeMs)
        {
            player.Setup(p => p.CurrentItem).Returns(item);
            player.Setup(p => p.IsPlaying).Returns(isPlaying);
            player.Setup(p => p.GetTotalTimeMs()).Returns(totalTimeMs);
            player.SetupProperty(p => p.Volume);
        }

        [Test]
        public void Play_LongSongOnly_ShouldPlayWithoutCrossfade()
        {
            // Arrange
            var song = CreateItem("Long.mp3", (int)MinLongDurationMs);
            SetupPlayer(_playerA, song, false, song.SoundFile.DurationMs);

            // Act
            _controller.Play(song);

            // Assert
            _playerA.Verify(p => p.Play(song, StartSeconds), Times.Once);
            // Initial Play calls StopAll, so Stop will be called on both.
            _playerA.Verify(p => p.Stop(), Times.Once);
            _playerB.Verify(p => p.Stop(), Times.Once);
            Assert.That(_playerA.Object.Volume, Is.EqualTo(1.0f).Within(0.01f));
        }

        [Test]
        public void Play_ShortSongOnly_ShouldPlayFromBeginning()
        {
            // Arrange
            var song = CreateItem("Short.mp3", (int)MinLongDurationMs - 1000);
            SetupPlayer(_playerA, song, false, song.SoundFile.DurationMs);

            // Act
            _controller.Play(song);

            // Assert
            _playerA.Verify(p => p.Play(song, 0.0), Times.Once);
            Assert.That(_playerA.Object.Volume, Is.EqualTo(1.0f).Within(0.01f));
        }

        [Test]
        public void Play_LongToLong_ShouldCrossfade()
        {
            // Arrange
            var song1 = CreateItem("Long1.mp3", (int)MinLongDurationMs);
            var song2 = CreateItem("Long2.mp3", (int)MinLongDurationMs);

            // 第一の曲をセットして再生中状態を作る
            SetupPlayer(_playerA, song1, true, song1.SoundFile.DurationMs);

            // song2 の情報をモックにセット（CanExecuteCrossfade用）
            _playerB.Setup(p => p.GetTotalTimeMs()).Returns(song2.SoundFile.DurationMs);

            // Act
            _controller.Play(song2);

            // Assert
            _playerB.Verify(p => p.Play(song2, StartSeconds), Times.Once);
            Assert.That(_playerB.Object.Volume, Is.EqualTo(0.0f).Within(0.01f));
            _playerA.Verify(p => p.Stop(), Times.Never); // A is still fading out
        }

        [Test]
        public void Play_ShortToShort_ShouldNotCrossfade()
        {
            // Arrange
            var song1 = CreateItem("Short1.mp3", (int)MinLongDurationMs - 1000);
            var song2 = CreateItem("Short2.mp3", (int)MinLongDurationMs - 1000);

            _playerA.Setup(p => p.IsPlaying).Returns(true);
            _playerA.Setup(p => p.CurrentItem).Returns(song1);
            _playerA.SetupProperty(p => p.Volume);

            // Act
            _controller.Play(song2);

            // Assert
            _playerA.Verify(p => p.Stop(), Times.AtLeastOnce());
            _playerA.Verify(p => p.Play(song2, 0.0), Times.Once);
            Assert.That(_playerA.Object.Volume, Is.EqualTo(1.0f).Within(0.01f));
        }

        [Test]
        public void Play_LongToShort_ShouldNotCrossfade()
        {
            // Arrange
            var song1 = CreateItem("Long.mp3", (int)MinLongDurationMs);
            var song2 = CreateItem("Short.mp3", (int)MinLongDurationMs - 1000);

            _playerA.Setup(p => p.IsPlaying).Returns(true);
            _playerA.Setup(p => p.CurrentItem).Returns(song1);
            _playerA.SetupProperty(p => p.Volume);

            // Act
            _controller.Play(song2);

            // Assert
            _playerA.Verify(p => p.Stop(), Times.AtLeastOnce());
            _playerA.Verify(p => p.Play(song2, 0.0), Times.Once);
        }

        [Test]
        public void Play_ShortToLong_ShouldNotCrossfade()
        {
            // Arrange
            var song1 = CreateItem("Short.mp3", (int)MinLongDurationMs - 1000);
            var song2 = CreateItem("Long.mp3", (int)MinLongDurationMs);

            // 既に playerA で song1 を再生中とする（ただし Play を呼ばずにセットする）
            // CrossfadeController のコンストラクタで activePlayer = playerA, fadingOutPlayer = playerB になっている

            _playerA.Setup(p => p.IsPlaying).Returns(true);
            _playerA.Setup(p => p.CurrentItem).Returns(song1);
            _playerA.SetupProperty(p => p.Volume);

            // Act
            _controller.Play(song2);

            // Assert
            // CanExecuteCrossfade(song2) は true (長いため)
            // activePlayer.IsPlaying も true
            // よって if (activePlayer.IsPlaying && canCrossfade) に入り、クロスフェードが実行される
            _playerB.Verify(p => p.Play(song2, StartSeconds), Times.Once);
            Assert.That(_playerB.Object.Volume, Is.EqualTo(0.0f).Within(0.01f));
        }
    }
}