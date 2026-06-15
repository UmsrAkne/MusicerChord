using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MusicerChord.Core;
using MusicerChord.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MusicerChord.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class DirectoryTreeViewModel : BindableBase, IDisposable
    {
        private readonly SemaphoreSlim semaphore = new (1, 1);
        private ObservableCollection<ISoundContainer> soundContainers = new ();

        private ISoundContainer selectedContainer;
        private bool disposed;

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

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                semaphore.Dispose();
            }

            disposed = true;
        }

        private async Task OnRequestInsertChildren(ISoundContainer parent, IEnumerable<ISoundContainer> children)
        {
            await semaphore.WaitAsync();

            try
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
                    child.RequestInsertChildren = OnRequestInsertChildren;
                    SoundContainers.Insert(insertIndex++, child);
                    await Task.Delay(40);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}