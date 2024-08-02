namespace Delta.Domain.Generator.similarity
{
    public class Crc32SimilarityFinder : SimilarityFinder
    {
        /// <summary>
        /// All entries in the base archive, organized by CRC32.
        /// </summary>
        private readonly Dictionary<long, List<MinimalZipEntry>> baseEntriesByCrc32 = [];

        /// <summary>
        /// Constructs a new similarity finder with the specified parameters.
        /// </summary>
        /// <param name="baseArchive">The base archive that contains the entries to be searched.</param>
        /// <param name="baseEntries">The entries in the base archive that are eligible to be searched.</param>
        public Crc32SimilarityFinder(FileInfo baseArchive, ICollection<MinimalZipEntry> baseEntries)
            : base(baseArchive, baseEntries)
        {
            foreach (var oldEntry in baseEntries)
            {
                long crc32 = oldEntry.GetCrc32OfUncompressedData();
                if (!baseEntriesByCrc32.TryGetValue(crc32, out var entriesForCrc32))
                {
                    entriesForCrc32 = new List<MinimalZipEntry>();
                    baseEntriesByCrc32[crc32] = entriesForCrc32;
                }
                entriesForCrc32.Add(oldEntry);
            }
        }

        /// <summary>
        /// Searches for files similar to the specified entry in the specified new archive against all of
        /// the available entries in the base archive.
        /// </summary>
        /// <param name="newArchive">The new archive that contains the new entry.</param>
        /// <param name="newEntry">The new entry to compare against the entries in the base archive.</param>
        /// <returns>A <see cref="List{MinimalZipEntry}"/> of entries (possibly empty but never null) from
        /// the base archive that are similar to the new archive; if the list has more than one entry, the
        /// entries should be in order from most similar to least similar.</returns>
        public override List<MinimalZipEntry> FindSimilarFiles(FileInfo newArchive, MinimalZipEntry newEntry)
        {
            if (baseEntriesByCrc32.TryGetValue(newEntry.GetCrc32OfUncompressedData(), out var matchedEntries))
            {
                return new List<MinimalZipEntry>(matchedEntries);
            }

            return new List<MinimalZipEntry>();
        }
    }
}