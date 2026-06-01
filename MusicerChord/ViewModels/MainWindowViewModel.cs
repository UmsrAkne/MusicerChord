using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using MusicerChord.Models;
using MusicerChord.Utils;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #if DEBUG
        // ReSharper disable once UnusedMember.Local
        private readonly string testDirectoryPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                @"myFiles\tests\RiderProjects\MusicerChord");
        #endif

        private readonly AppVersionInfo appVersionInfo = new ();

        public MainWindowViewModel(
            SoundListViewModel soundListViewModel,
            DirectoryTreeViewModel directoryTreeViewModel)
        {
            SoundListViewModel = soundListViewModel;
            DirectoryTreeViewModel = directoryTreeViewModel;

            SetupDummyData();
        }

        public MainWindowViewModel()
        {
            SoundListViewModel = new SoundListViewModel();
            DirectoryTreeViewModel = new DirectoryTreeViewModel();

            SetupDummyData();
        }

        public SoundListViewModel SoundListViewModel { get; private set; }

        public DirectoryTreeViewModel DirectoryTreeViewModel { get; private set; }

        public string Title => appVersionInfo.Title;

        [Conditional("DEBUG")]
        private void SetupDummyData()
        {
            SoundListViewModel.SoundFiles.Add(new SoundFile { RelativePath = "test1.mp3", DurationMs = 1000, });
            SoundListViewModel.SoundFiles.Add(new SoundFile { RelativePath = "test2.mp3", DurationMs = 1000, });
            SoundListViewModel.SoundFiles.Add(new SoundFile { RelativePath = "test3.mp3", DurationMs = 1000, });

            var dir1 = new DirectorySoundSource(@"Dummy\Container_1", @"C:\DummyPath")
            {
                Children = new ObservableCollection<ISoundContainer>
                {
                    new DirectorySoundSource(@"Dummy\Container_1\SubContainer_1", @"C:\DummyPath"),
                    new DirectorySoundSource(@"Dummy\Container_1\SubContainer_2", @"C:\DummyPath"),
                },
            };

            var dir2 = new DirectorySoundSource(@"Dummy\Container_1", @"C:\DummyPath")
            {
                Children = new ObservableCollection<ISoundContainer>
                {
                    new DirectorySoundSource(@"Dummy\Container_2\SubContainer_1", @"C:\DummyPath"),
                    new DirectorySoundSource(@"Dummy\Container_2\SubContainer_2", @"C:\DummyPath"),
                },
            };

            var dir3 = new DirectorySoundSource(@"Dummy\Container_1", @"C:\DummyPath")
            {
                Children = new ObservableCollection<ISoundContainer>
                {
                    new DirectorySoundSource(@"Dummy\Container_3\SubContainer_1", @"C:\DummyPath"),
                    new DirectorySoundSource(@"Dummy\Container_3\SubContainer_2", @"C:\DummyPath"),
                },
            };

            DirectoryTreeViewModel.SoundContainers.Add(dir1);
            DirectoryTreeViewModel.SoundContainers.Add(dir2);
            DirectoryTreeViewModel.SoundContainers.Add(dir3);
            DirectoryTreeViewModel.SoundContainers.Add(new DirectorySoundSource(@"Dummy\Container_4", @"C:\DummyPath"));
        }
    }
}