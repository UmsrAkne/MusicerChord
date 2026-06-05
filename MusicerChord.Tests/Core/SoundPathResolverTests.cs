using MusicerChord.Core;
using MusicerChord.Models;

namespace MusicerChord.Tests.Core
{
    [TestFixture]
    public class SoundPathResolverTests
    {
        [Test]
        public void ResolveFullPath_ShouldCombineRootAndRelativePath()
        {
            // Arrange
            var root = @"D:\Sound";
            var relative = @"Ambient\01_rain.mp3";
            var resolver = new SoundPathResolver(root);
            var soundFile = new SoundFile { RelativePath = relative, };

            // Act
            var result = resolver.ResolveFullPath(soundFile);

            // Assert
            var expected = Path.Combine(root, relative);
            TestContext.WriteLine($"expected: {expected}");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ResolveFullPath_ShouldHandleRootWithTrailingSeparator()
        {
            // Arrange
            var root = @"D:\Sound\";
            var relative = @"Ambient\01_rain.mp3";
            var resolver = new SoundPathResolver(root);
            var soundFile = new SoundFile { RelativePath = relative, };

            // Act
            var result = resolver.ResolveFullPath(soundFile);

            // Assert
            var expected = Path.Combine(root, relative);
            TestContext.WriteLine($"expected: {expected}");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ResolveFullPath_ShouldHandleEmptyRelativePath()
        {
            // Arrange
            var root = @"D:\Sound";
            var relative = "";
            var resolver = new SoundPathResolver(root);
            var soundFile = new SoundFile { RelativePath = relative, };

            // Act
            var result = resolver.ResolveFullPath(soundFile);

            // Assert
            var expected = root;
            TestContext.WriteLine($"expected: {expected}");

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}