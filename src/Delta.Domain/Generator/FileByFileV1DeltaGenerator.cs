using Delta.Domain.Generator.bsdiff;

namespace Delta.Domain.Generator
{
    public class FileByFileV1DeltaGenerator : IDeltaGenerator
    {
        private readonly IList<IRecommendationModifier> _recommendationModifiers;

        public FileByFileV1DeltaGenerator(params IRecommendationModifier[] recommendationModifiers)
        {
            _recommendationModifiers = recommendationModifiers?.ToList() ?? [];
        }

        public void GenerateDeltaAsync(FileInfo oldBlob, FileInfo newBlob, Stream deltaOut)
        {
            try
            {
                using var deltaFriendlyOldFile = new TempFileHolder();
                using var deltaFriendlyNewFile = new TempFileHolder();
                using var deltaFile = new TempFileHolder();
                using var deltaFileOut = new FileStream(deltaFile.File.FullName, FileMode.Create, FileAccess.Write);
                using var bufferedDeltaOut = new BufferedStream(deltaFileOut);

                var builder = new PreDiffExecutor.Builder()
                    .ReadingOriginalFiles(oldFile, newFile)
                    .WritingDeltaFriendlyFiles(deltaFriendlyOldFile.File, deltaFriendlyNewFile.File);

                foreach (var modifier in _recommendationModifiers)
                {
                    builder.WithRecommendationModifier(modifier);
                }

                var executor = builder.Build();
                var preDiffPlan = executor.PrepareForDiffing();
                var deltaGenerator = GetDeltaGenerator();
                deltaGenerator.GenerateDeltaAsync(deltaFriendlyOldFile.File, deltaFriendlyNewFile.File, bufferedDeltaOut);

                bufferedDeltaOut.Flush();
                var patchWriter = new PatchWriter(
                    preDiffPlan,
                    deltaFriendlyOldFile.File.Length,
                    deltaFriendlyNewFile.File.Length,
                    deltaFile.File);

                patchWriter.WriteV1Patch(patchOut);
            }
            catch (IOException ex)
            {
                throw new IOException("An I/O error occurred while generating the delta.", ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new OperationCanceledException("The operation was canceled.", ex);
            }
        }

        public PreDiffPlan GeneratePreDiffPlan(FileInfo oldFile, FileInfo newFile)
        {
            try
            {
                using var deltaFriendlyOldFile = new TempFileHolder();
                using var deltaFriendlyNewFile = new TempFileHolder();

                var builder = new PreDiffExecutor.Builder()
                    .ReadingOriginalFiles(oldFile, newFile)
                    .WritingDeltaFriendlyFiles(deltaFriendlyOldFile.File, deltaFriendlyNewFile.File);

                foreach (var modifier in _recommendationModifiers)
                {
                    builder.WithRecommendationModifier(modifier);
                }

                var executor = builder.Build();
                return executor.PrepareForDiffing();
            }
            catch (IOException ex)
            {
                throw new IOException("An I/O error occurred while generating the pre-diffing plan.", ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new OperationCanceledException("The operation was canceled.", ex);
            }
        }

        protected virtual IDeltaGenerator GetDeltaGenerator()
        {
            return new BsDiffDeltaGenerator();
        }
    }
}