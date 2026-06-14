using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DirectoryTreeViewModel : BindableBase
    {
        private ObservableCollection<ISoundContainer> soundContainers = new ();

        private ISoundContainer selectedContainer;

        public event Action<ISoundContainer> SoundContainerOpened;

        public ObservableCollection<ISoundContainer> SoundContainers
        {
            get => soundContainers;
            private set => SetProperty(ref soundContainers, value);
        }

        public ISoundContainer SelectedContainer
        {
            get => selectedContainer;
            set => SetProperty(ref selectedContainer, value);
        }

        public DelegateCommand OpenSoundContainerCommand => new (() =>
        {
            Console.WriteLine("Open Sound Container");
            SoundContainerOpened?.Invoke(SelectedContainer);
        });

        public void AddSoundContainers(IEnumerable<ISoundContainer> containers)
        {
            var l = containers.ToList();
            foreach (var s in l)
            {
                s.RequestInsertChildren = OnRequestInsertChildren;
            }

            SoundContainers.AddRange(l);
        }

        public async Task LoadDirectories(string rootPath)
        {
            var containers =
                await Task.Run(() => SoundSourceFactory.CreateFromPath(rootPath));

            AddSoundContainers(containers);
        }

        private void OnRequestInsertChildren(ISoundContainer parent, IEnumerable<ISoundContainer> children)
        {
            // 1. まず、大元のリストの中で「クリックされた親」が何番目にいるか探す
            var parentIndex = SoundContainers.IndexOf(parent);
            if (parentIndex == -1)
            {
                return;
            }

            // 2. 親のすぐ後ろのインデックスから、1件ずつ順番に挿入（Insert）していく
            var insertIndex = parentIndex + 1;
            foreach (var child in children)
            {
                SoundContainers.Insert(insertIndex++, child);
            }
        }
    }
}