using Delta.Domain.Generator.shared;

namespace Delta.Domain.Generator
{
    public class PatchWriter
    {
        private readonly PreDiffPlan plan;
        private readonly long deltaFriendlyOldFileSize;
        private readonly long deltaFriendlyNewFileSize;
        private readonly FileInfo deltaFile;

        public PatchWriter(PreDiffPlan plan, long deltaFriendlyOldFileSize, long deltaFriendlyNewFileSize, FileInfo deltaFile)
        {
            this.plan = plan;
            this.deltaFriendlyOldFileSize = deltaFriendlyOldFileSize;
            this.deltaFriendlyNewFileSize = deltaFriendlyNewFileSize;
            this.deltaFile = deltaFile;
        }

        public void WriteV1Patch(Stream outStream)
        {
            using var dataOut = new BinaryWriter(outStream);

            dataOut.Write(System.Text.Encoding.ASCII.GetBytes(PatchConstants.IDENTIFIER));
            dataOut.Write(0);
            dataOut.Write(deltaFriendlyOldFileSize);
            dataOut.Write(plan.GetOldFileUncompressionPlan().Count);

            foreach (var range in plan.GetOldFileUncompressionPlan())
            {
                dataOut.Write(range.Offset);
                dataOut.Write(range.Length);
            }

            dataOut.Write(plan.GetDeltaFriendlyNewFileRecompressionPlan().Count);

            foreach (var range in plan.GetDeltaFriendlyNewFileRecompressionPlan())
            {
                dataOut.Write(range.Offset);
                dataOut.Write(range.Length);
                dataOut.Write((int)PatchConstants.CompatibilityWindowId.DEFAULT_DEFLATE);
                dataOut.Write(range.Metadata.Level);
                dataOut.Write(range.Metadata.Strategy);
                dataOut.Write(range.Metadata.Nowrap ? 1 : 0);
            }

            dataOut.Write(1);
            dataOut.Write((int)PatchConstants.DeltaFormat.BSDIFF);

            dataOut.Write(0L);
            dataOut.Write(deltaFriendlyOldFileSize);
            dataOut.Write(0L);
            dataOut.Write(deltaFriendlyNewFileSize);

            dataOut.Write(deltaFile.Length);

            using (var deltaFileIn = new FileStream(deltaFile.FullName, FileMode.Open, FileAccess.Read))

            using (var deltaIn = new BufferedStream(deltaFileIn))
            {
                byte[] buffer = new byte[32768];
                int numRead;
                while ((numRead = deltaIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    dataOut.Write(buffer, 0, numRead);
                }
            }

            dataOut.Flush();
        }
    }
}
