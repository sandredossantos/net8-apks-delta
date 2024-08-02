using Delta.Domain.Generator.shared;
using Delta.Domain.Generator.similarity;

namespace Delta.Domain.Generator
{
    public class PreDiffPlanner
    {
        private readonly FileInfo oldFile;
        private readonly FileInfo newFile;
        private readonly Dictionary<ByteArrayHolder, MinimalZipEntry> oldArchiveZipEntriesByPath;
        private readonly Dictionary<ByteArrayHolder, MinimalZipEntry> newArchiveZipEntriesByPath;
        private readonly Dictionary<ByteArrayHolder, JreDeflateParameters> newArchiveJreDeflateParametersByPath;
        private readonly List<IRecommendationModifier> recommendationModifiers;

        public PreDiffPlanner(
            FileInfo oldFile,
            Dictionary<ByteArrayHolder, MinimalZipEntry> oldArchiveZipEntriesByPath,
            FileInfo newFile,
            Dictionary<ByteArrayHolder, MinimalZipEntry> newArchiveZipEntriesByPath,
            Dictionary<ByteArrayHolder, JreDeflateParameters> newArchiveJreDeflateParametersByPath,
            params IRecommendationModifier[] recommendationModifiers)
        {
            this.oldFile = oldFile;
            this.oldArchiveZipEntriesByPath = oldArchiveZipEntriesByPath;
            this.newFile = newFile;
            this.newArchiveZipEntriesByPath = newArchiveZipEntriesByPath;
            this.newArchiveJreDeflateParametersByPath = newArchiveJreDeflateParametersByPath;
            this.recommendationModifiers = new List<IRecommendationModifier>(recommendationModifiers);
        }

        public PreDiffPlan GeneratePreDiffPlan()
        {
            var recommendations = GetDefaultRecommendations();

            foreach (var modifier in recommendationModifiers)
            {
                recommendations = modifier.GetModifiedRecommendations(oldFile, newFile, recommendations);
            }

            var oldFilePlan = new HashSet<TypedRange<object>>();
            var newFilePlan = new HashSet<TypedRange<JreDeflateParameters>>();

            foreach (var recommendation in recommendations)
            {
                if (recommendation.GetRecommendation() == Recommendation.UncompressOld)
                {
                    var offset = recommendation.GetOldEntry().GetFileOffsetOfCompressedData();
                    var length = recommendation.GetOldEntry().CompressedSize;
                    var range = new TypedRange<object>(offset, length, null);
                    oldFilePlan.Add(range);
                }
                if (recommendation.GetRecommendation() == Recommendation.UncompressNew)
                {
                    var offset = recommendation.GetNewEntry().GetFileOffsetOfCompressedData();
                    var length = recommendation.GetNewEntry().CompressedSize;
                    var newJreDeflateParameters = newArchiveJreDeflateParametersByPath[
                        new ByteArrayHolder(recommendation.GetNewEntry().GetFileNameBytes())];
                    var range = new TypedRange<JreDeflateParameters>(offset, length, newJreDeflateParameters);
                    newFilePlan.Add(range);
                }
            }

            var oldFilePlanList = oldFilePlan.ToList();
            oldFilePlanList.Sort();
            var newFilePlanList = newFilePlan.ToList();
            newFilePlanList.Sort();

            return new PreDiffPlan(
                recommendations.AsReadOnly(),
                oldFilePlanList.AsReadOnly(),
                newFilePlanList.AsReadOnly());
        }

        private List<QualifiedRecommendation> GetDefaultRecommendations()
        {
            var recommendations = new List<QualifiedRecommendation>();

            var trivialRenameFinder = new Crc32SimilarityFinder(oldFile, oldArchiveZipEntriesByPath.Values);

            foreach (var newEntry in newArchiveZipEntriesByPath)
            {
                var newEntryPath = newEntry.Key;

                if (!oldArchiveZipEntriesByPath.TryGetValue(newEntryPath, out var oldZipEntry))
                {
                    var identicalEntriesInOldArchive = trivialRenameFinder.FindSimilarFiles(newFile, newEntry.Value);

                    if (identicalEntriesInOldArchive.Any())
                    {
                        oldZipEntry = identicalEntriesInOldArchive[0];
                    }
                }

                if (oldZipEntry != null)
                {
                    recommendations.Add(GetRecommendation(oldZipEntry, newEntry.Value));
                }
            }
            return recommendations;
        }

        private QualifiedRecommendation GetRecommendation(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            if (UnsuitableDeflate(newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressNeither,
                    RecommendationReason.DEFLATE_UNSUITABLE);
            }

            if (Unsuitable(oldEntry, newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressNeither,
                    RecommendationReason.UNSUITABLE);
            }

            if (BothEntriesUncompressed(oldEntry, newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressNeither,
                    RecommendationReason.BOTH_ENTRIES_UNCOMPRESSED);
            }

            if (UncompressedChangedToCompressed(oldEntry, newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressNew,
                    RecommendationReason.UNCOMPRESSED_CHANGED_TO_COMPRESSED);
            }

            if (CompressedChangedToUncompressed(oldEntry, newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressOld,
                    RecommendationReason.COMPRESSED_CHANGED_TO_UNCOMPRESSED);
            }

            if (CompressedBytesChanged(oldEntry, newEntry))
            {
                return new QualifiedRecommendation(
                    oldEntry,
                    newEntry,
                    Recommendation.UncompressBoth,
                    RecommendationReason.COMPRESSED_BYTES_CHANGED);
            }

            return new QualifiedRecommendation(
                oldEntry,
                newEntry,
                Recommendation.UncompressNeither,
                RecommendationReason.COMPRESSED_BYTES_IDENTICAL);
        }

        private bool Unsuitable(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            if (oldEntry.CompressionMethod != 0 && !oldEntry.IsDeflateCompressed())
            {
                return true;
            }
            if (newEntry.CompressionMethod != 0 && !newEntry.IsDeflateCompressed())
            {
                return true;
            }
            return false;
        }

        private bool UnsuitableDeflate(MinimalZipEntry newEntry)
        {
            var newJreDeflateParameters = newArchiveJreDeflateParametersByPath[
                new ByteArrayHolder(newEntry.GetFileNameBytes())];
            if (newEntry.IsDeflateCompressed() && newJreDeflateParameters == null)
            {
                return true;
            }
            return false;
        }

        private bool BothEntriesUncompressed(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            return oldEntry.CompressionMethod == 0 && newEntry.CompressionMethod == 0;
        }

        private bool UncompressedChangedToCompressed(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            return oldEntry.CompressionMethod == 0 && newEntry.CompressionMethod != 0;
        }

        private bool CompressedChangedToUncompressed(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            return newEntry.CompressionMethod == 0 && oldEntry.CompressionMethod != 0;
        }

        private bool CompressedBytesChanged(MinimalZipEntry oldEntry, MinimalZipEntry newEntry)
        {
            if (oldEntry.CompressedSize != newEntry.CompressedSize)
            {
                return true;
            }
            
            var buffer = new byte[4096];
            int numRead;
            
            using var newRafis = new RandomAccessFileInputStream(newFile.FullName, newEntry.GetFileOffsetOfCompressedData(), newEntry.CompressedSize);
                        
            using var matcher = new MatchingOutputStream(new RandomAccessFileInputStream(oldFile.FullName, oldEntry.GetFileOffsetOfCompressedData(), oldEntry.CompressedSize), 4096);

            while ((numRead = newRafis.Read(buffer, 0, buffer.Length)) > 0)
            {
                try
                {
                    matcher.Write(buffer, 0, numRead);
                }
                catch (MismatchException)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
