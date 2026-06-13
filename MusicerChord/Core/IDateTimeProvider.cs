using System;

namespace MusicerChord.Core
{
    public interface IDateTimeProvider
    {
        public DateTime Now { get; }
    }
}