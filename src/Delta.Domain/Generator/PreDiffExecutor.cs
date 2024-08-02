using Delta.Domain.Generator.shared;

namespace Delta.Domain.Generator
{
    public class PreDiffExecutor
    {
        public class Builder
        {
            private FileInfo originalOldFile;
            private FileInfo originalNewFile;
            private FileInfo deltaFriendlyOldFile;
            private FileInfo deltaFriendlyNewFile;

            private readonly List<IRecommendationModifier> recommendationModifiers = new List<IRecommendationModifier>();

            public Builder ReadingOriginalFiles(FileInfo originalOldFile, FileInfo originalNewFile)
            {
                if (originalOldFile == null || originalNewFile == null)
                {
                    throw new ArgumentNullException("do not set null original input files");
                }
                this.originalOldFile = originalOldFile;
                this.originalNewFile = originalNewFile;
                return this;
            }

            public Builder WritingDeltaFriendlyFiles(FileInfo deltaFriendlyOldFile, FileInfo deltaFriendlyNewFile)
            {
                if (deltaFriendlyOldFile == null || deltaFriendlyNewFile == null)
                {
                    throw new ArgumentNullException("do not set null delta-friendly files");
                }
                this.deltaFriendlyOldFile = deltaFriendlyOldFile;
                this.deltaFriendlyNewFile = deltaFriendlyNewFile;
                return this;
            }

            public Builder WithRecommendationModifier(IRecommendationModifier recommendationModifier)
            {
                if (recommendationModifier == null)
                {
                    throw new ArgumentNullException("recommendationModifier cannot be null");
                }
                this.recommendationModifiers.Add(recommendationModifier);
                return this;
            }

            public PreDiffExecutor Build()
            {
                if (originalOldFile == null)
                {
                    throw new InvalidOperationException("original input files cannot be null");
                }
                return new PreDiffExecutor(
                    originalOldFile,
                    originalNewFile,
                    deltaFriendlyOldFile,
                    deltaFriendlyNewFile,
                    recommendationModifiers);
            }
        }

        private readonly FileInfo originalOldFile;
        private readonly FileInfo originalNewFile;
        private readonly FileInfo deltaFriendlyOldFile;
        private readonly FileInfo deltaFriendlyNewFile;
        private readonly List<IRecommendationModifier> recommendationModifiers;

        private PreDiffExecutor(
            FileInfo originalOldFile,
            FileInfo originalNewFile,
            FileInfo deltaFriendlyOldFile,
            FileInfo deltaFriendlyNewFile,
            List<IRecommendationModifier> recommendationModifiers)
        {
            this.originalOldFile = originalOldFile;
            this.originalNewFile = originalNewFile;
            this.deltaFriendlyOldFile = deltaFriendlyOldFile;
            this.deltaFriendlyNewFile = deltaFriendlyNewFile;
            this.recommendationModifiers = recommendationModifiers;
        }

        public PreDiffPlan PrepareForDiffing()
        {
            PreDiffPlan preDiffPlan = GeneratePreDiffPlan();
            List<TypedRange<JreDeflateParameters>> deltaFriendlyNewFileRecompressionPlan = null;
            if (deltaFriendlyOldFile != null)
            {
                // Builder.WritingDeltaFriendlyFiles() ensures old and new are non-null when called, so a
                // check on either is sufficient.
                deltaFriendlyNewFileRecompressionPlan =
                    GenerateDeltaFriendlyFiles(preDiffPlan).AsReadOnly();
            }
            return new PreDiffPlan(
                preDiffPlan.QualifiedRecommendations,
                preDiffPlan.OldFileUncompressionPlan,
                preDiffPlan.NewFileUncompressionPlan,
                deltaFriendlyNewFileRecompressionPlan);
        }

        /// <summary>
        /// Generate the delta-friendly files and return the plan for recompressing the delta-friendly new
        /// file back into the original new file.
        /// </summary>
        /// <param name="preDiffPlan">The plan to execute</param>
        /// <returns>As described</returns>
        /// <exception cref="IOException">If anything goes wrong</exception>
        private List<TypedRange<JreDeflateParameters>> GenerateDeltaFriendlyFiles(PreDiffPlan preDiffPlan)
        {
            using (var fsOld = new FileStream(deltaFriendlyOldFile.FullName, FileMode.Create, FileAccess.Write))
            using (var bufferedOutOld = new BufferedStream(fsOld))
            {
                DeltaFriendlyFile.GenerateDeltaFriendlyFile(
                    preDiffPlan.OldFileUncompressionPlan, originalOldFile, bufferedOutOld);
            }
            using (var fsNew = new FileStream(deltaFriendlyNewFile.FullName, FileMode.Create, FileAccess.Write))
            using (var bufferedOutNew = new BufferedStream(fsNew))
            {
                return DeltaFriendlyFile.GenerateDeltaFriendlyFile(
                    preDiffPlan.NewFileUncompressionPlan, originalNewFile, bufferedOutNew);
            }
        }

        private PreDiffPlan GeneratePreDiffPlan()
        {
            var originalOldArchiveZipEntriesByPath = new Dictionary<ByteArrayHolder, MinimalZipEntry>();
            var originalNewArchiveZipEntriesByPath = new Dictionary<ByteArrayHolder, MinimalZipEntry>();
            var originalNewArchiveJreDeflateParametersByPath = new Dictionary<ByteArrayHolder, JreDeflateParameters>();

            foreach (var zipEntry in MinimalZipArchive.ListEntries(originalOldFile))
            {
                var key = new ByteArrayHolder(zipEntry.FileNameBytes);
                originalOldArchiveZipEntriesByPath[key] = zipEntry;
            }

            var diviner = new DefaultDeflateCompressionDiviner();
            foreach (var divinationResult in diviner.DivineDeflateParameters(originalNewFile))
            {
                var key = new ByteArrayHolder(divinationResult.MinimalZipEntry.FileNameBytes);
                originalNewArchiveZipEntriesByPath[key] = divinationResult.MinimalZipEntry;
                originalNewArchiveJreDeflateParametersByPath[key] = divinationResult.DivinedParameters;
            }

            var preDiffPlanner = new PreDiffPlanner(
                originalOldFile,
                originalOldArchiveZipEntriesByPath,
                originalNewFile,
                originalNewArchiveZipEntriesByPath,
                originalNewArchiveJreDeflateParametersByPath,
                recommendationModifiers.ToArray());
            return preDiffPlanner.GeneratePreDiffPlan();
        }
    }
}
