using Moq;
using MusicerChord.Core;
using MusicerChord.Models;

namespace MusicerChord.Tests.Core
{
    [TestFixture]
    public class CrossFadeControllerV2Tests
    {
        private Mock<ISoundPlayer> _player1;
        private Mock<ISoundPlayer> _player2;
        private Mock<ISoundPlayer> _player3;

        // ファクトリ自体のモックを用意する
        private Mock<ISoundPlayerFactory> _factoryMock;
        private CrossFadeControllerV2 _controller;

        [SetUp]
        public void Setup()
        {
            _player1 = new Mock<ISoundPlayer>();
            _player2 = new Mock<ISoundPlayer>();
            _player3 = new Mock<ISoundPlayer>();

            // ファクトリのモックを作成（※SoundPlayerFactoryがインターフェース、またはVirtualなCreateメソッドを持つ前提）
            _factoryMock = new Mock<ISoundPlayerFactory>();

            // 呼び出されるたびに、順番に player1 -> player2 -> player3 を返すように設定
            _factoryMock.SetupSequence(f => f.Create())
                .Returns(_player1.Object)
                .Returns(_player2.Object)
                .Returns(_player3.Object);

            // 新しいコンストラクタに合わせてファクトリのモックを渡す
            _controller = new CrossFadeControllerV2(_factoryMock.Object);
        }

        private SoundPlaybackItem CreateItem(string name, int durationMs)
        {
            return new SoundPlaybackItem(new SoundFile { RelativePath = name, DurationMs = durationMs });
        }

        [Test]
        public void Play_ShouldUsePlayersInOrder()
        {
            // Arrange
            var item1 = CreateItem("Song1.mp3", 60000);
            var item2 = CreateItem("Song2.mp3", 60000);
            var item3 = CreateItem("Song3.mp3", 60000);

            // Act & Assert 1 (1回目のPlay -> player1が使われるはず)
            _controller.Play(item1);
            _player1.Verify(p => p.Play(item1, 0), Times.Once); // 元のコードの引数に合わせて調整してください

            // Act & Assert 2 (2回目のPlay -> player2が使われるはず)
            _controller.Play(item2);
            _player2.Verify(p => p.Play(item2, 0), Times.Once);

            // Act & Assert 3 (3回目のPlay -> player3が使われるはず)
            _controller.Play(item3);
            _player3.Verify(p => p.Play(item3, 0), Times.Once);

            // ファクトリが計3回呼ばれたことも検証できる
            _factoryMock.Verify(f => f.Create(), Times.Exactly(3));
        }

        [Test]
        public void PlaybackStopped_ShouldTriggerNextTrackRequested()
        {
            // Arrange
            bool eventRaised = false;
            _controller.NextTrackRequested += () => eventRaised = true;
            _controller.Play(CreateItem("Song1.mp3", 60000));

            // Act
            _player1.Raise(p => p.PlaybackStopped += null, EventArgs.Empty);

            // Assert
            Assert.IsTrue(eventRaised, "NextTrackRequested should be raised when PlaybackStopped is fired.");
        }

        [Test]
        public void SequentialPlay_DrivenByEvent_ShouldWork()
        {
            // Arrange
            var item1 = CreateItem("Song1.mp3", 60000);
            var item2 = CreateItem("Song2.mp3", 60000);
            
            int requestedCount = 0;
            _controller.NextTrackRequested += () =>
            {
                requestedCount++;
                if (requestedCount == 1)
                {
                    _controller.Play(item2);
                }
            };

            // Act
            _controller.Play(item1);
            _player1.Raise(p => p.PlaybackStopped += null, EventArgs.Empty);

            // Assert
            _player1.Verify(p => p.Play(item1, 0), Times.Once);
            _player2.Verify(p => p.Play(item2, 0), Times.Once);
            Assert.That(requestedCount, Is.EqualTo(1));
        }

        [Test]
        [TestCase(20.0, true)]  // 5 + 5 + 10 = 20
        [TestCase(19.9, false)]
        [TestCase(21.0, true)]
        public void CanExecuteCrossfade_ShouldRespectDurations(double totalDurationSeconds, bool expected)
        {
            // Arrange
            var item = CreateItem("Test.mp3", (int)(totalDurationSeconds * 1000));
            _controller.StartSeconds = 5.0;
            _controller.EndOffsetSeconds = 5.0;
            _controller.CrossfadeDurationSeconds = 10.0;

            // Act
            var result = _controller.CanExecuteCrossfade(item);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Update_ShouldUpdatePlaybackPropertiesFromLatestPlayer()
        {
            // Arrange
            var item1 = CreateItem("Song1.mp3", 60000);
            var item2 = CreateItem("Song2.mp3", 90000);
            
            _player1.Setup(p => p.CurrentItem).Returns(item1);
            _player1.Setup(p => p.GetPlaybackTimeMs()).Returns(1000);
            _player1.Setup(p => p.GetTotalTimeMs()).Returns(60000);
            
            _player2.Setup(p => p.CurrentItem).Returns(item2);
            _player2.Setup(p => p.GetPlaybackTimeMs()).Returns(500);
            _player2.Setup(p => p.GetTotalTimeMs()).Returns(90000);

            // Act 1: Player1 のみ
            _controller.Play(item1);
            _controller.Update(0.1);

            // Assert 1
            Assert.That(_controller.CurrentItem, Is.EqualTo(item1));
            Assert.That(_controller.CurrentPlaybackTimeText, Is.EqualTo("00:00:01"));
            Assert.That(_controller.TotalTimeText, Is.EqualTo("00:01:00"));
            Assert.That(_controller.CurrentSoundName, Is.EqualTo("Song1"));
            _player1.Verify(p => p.UpdateItemState(), Times.Once);

            // Act 2: Player2 を追加 (クロスフェード開始想定)
            _controller.Play(item2);
            _controller.Update(0.1);

            // Assert 2: 最新の Player2 の情報が反映されていること
            Assert.That(_controller.CurrentItem, Is.EqualTo(item2));
            Assert.That(_controller.CurrentPlaybackTimeText, Is.EqualTo("00:00:00"));
            Assert.That(_controller.TotalTimeText, Is.EqualTo("00:01:30"));
            Assert.That(_controller.CurrentSoundName, Is.EqualTo("Song2"));
            
            // 全てのプレイヤーの状態が更新されていること
            _player1.Verify(p => p.UpdateItemState(), Times.Exactly(2));
            _player2.Verify(p => p.UpdateItemState(), Times.Once);
        }

        [Test]
        public void Update_WhenNoPlayers_ShouldClearPlaybackProperties()
        {
            // Act
            _controller.Update(0.1);

            // Assert
            Assert.IsNull(_controller.CurrentItem);
            Assert.That(_controller.CurrentPlaybackTimeText, Is.EqualTo("00:00:00"));
            Assert.That(_controller.TotalTimeText, Is.EqualTo("00:00:00"));
            Assert.That(_controller.CurrentSoundName, Is.Empty);
        }
    }
}