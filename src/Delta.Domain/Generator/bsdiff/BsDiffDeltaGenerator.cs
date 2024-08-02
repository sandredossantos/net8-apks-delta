namespace Delta.Domain.Generator.bsdiff
{
    public class BsDiffDeltaGenerator : IDeltaGenerator
    {
        /// <summary>
        /// The minimum match length to use for bsdiff.
        /// </summary>
        private const int MatchLengthBytes = 16;

        public async Task GenerateDeltaAsync(FileInfo oldBlob, FileInfo newBlob, Stream deltaOut)
        {
            await BsDiffPatchWriter.GeneratePatchAsync(oldBlob, newBlob, deltaOut, MatchLengthBytes);
        }
    }
}