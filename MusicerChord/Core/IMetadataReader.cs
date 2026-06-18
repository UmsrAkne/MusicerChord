namespace MusicerChord.Core
{
    public interface IMetadataReader
    {
        public string GetFileTitle(string absolutePath);
    }
}