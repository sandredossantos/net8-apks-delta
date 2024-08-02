namespace Delta.Domain.Generator
{
    public interface IDeltaGenerator
    {
        void GenerateDeltaAsync(FileInfo oldBlob, FileInfo newBlob, Stream deltaOut);
    }
}