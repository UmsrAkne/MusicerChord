namespace MusicerChord.Core
{
    public class SoundPlayerFactory : ISoundPlayerFactory
    {
        public ISoundPlayer Create()
        {
            return new SoundPlayer();
        }
    }
}