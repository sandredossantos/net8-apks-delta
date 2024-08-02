namespace Delta.Domain.Generator.similarity
{
    public abstract class SimilarityFinder
    {
        /// <summary>
        /// The base archive that contains the entries to be searched.
        /// </summary>
        protected readonly FileInfo BaseArchive;

        /// <summary>
        /// The entries in the base archive that are eligible to be searched.
        /// </summary>
        protected readonly ICollection<MinimalZipEntry> BaseEntries;

        /// <summary>
        /// Create a new instance to check for similarity of arbitrary files against the specified entries
        /// in the specified archive.
        /// </summary>
        /// <param name="baseArchive">The base archive that contains the entries to be scored against.</param>
        /// <param name="baseEntries">The entries in the base archive that are eligible to be scored against.</param>
        protected SimilarityFinder(FileInfo baseArchive, ICollection<MinimalZipEntry> baseEntries)
        {
            BaseArchive = baseArchive;
            BaseEntries = baseEntries;
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
        public abstract List<MinimalZipEntry> FindSimilarFiles(FileInfo newArchive, MinimalZipEntry newEntry);
    }
}