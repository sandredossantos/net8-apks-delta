using System.Text;

namespace Delta.Domain.Generator.bsdiff
{
    public static class BsDiffPatchWriter
    {
        private const int DefaultMinimumMatchLength = 16;

        private static async Task WriteEntryAsync(
            IRandomAccessObject newData,
            IRandomAccessObject oldData,
            int newPosition,
            int oldPosition,
            int diffLength,
            int extraLength,
            int oldPositionOffsetForNextEntry,
            Stream outputStream)
        {
            // Write control data
            await BsUtil.WriteFormattedLongAsync(diffLength, outputStream);
            await BsUtil.WriteFormattedLongAsync(extraLength, outputStream);
            await BsUtil.WriteFormattedLongAsync(oldPositionOffsetForNextEntry, outputStream);

            await newData.SeekAsync(newPosition);
            await oldData.SeekAsync(oldPosition);

            // Write diff data
            for (int i = 0; i < diffLength; ++i)
            {
                outputStream.WriteByte((byte)(await newData.ReadUnsignedByteAsync() - await oldData.ReadUnsignedByteAsync()));
            }

            if (extraLength > 0)
            {
                await newData.SeekAsync(newPosition + diffLength);
                for (int i = 0; i < extraLength; ++i)
                {
                    outputStream.WriteByte(await newData.ReadByteAsync());
                }
            }
        }

        private static async Task GeneratePatchWithMatcherAsync(
            IRandomAccessObject oldData,
            IRandomAccessObject newData,
            Matcher matcher,
            Stream outputStream)
        {
            int lastNewPosition = 0;
            int lastOldPosition = 0;

            int newPosition = 0;
            int oldPosition = 0;

            while (newPosition < newData.Length)
            {
                if (Thread.CurrentThread.IsInterrupted)
                {
                    throw new OperationCanceledException();
                }

                var nextMatch = await matcher.NextAsync();
                if (nextMatch.DidFindMatch)
                {
                    newPosition = nextMatch.NewPosition;
                    oldPosition = nextMatch.OldPosition;
                }
                else
                {
                    newPosition = (int)newData.Length;
                }

                int backwardExtension = 0;
                if (newPosition < newData.Length)
                {
                    int score = 0;
                    int bestScore = 0;
                    for (int i = 1; newPosition - i >= lastNewPosition && oldPosition >= i; ++i)
                    {
                        await oldData.SeekAsync(oldPosition - i);
                        await newData.SeekAsync(newPosition - i);
                        if (await oldData.ReadByteAsync() == await newData.ReadByteAsync())
                        {
                            ++score;
                        }
                        else
                        {
                            --score;
                        }

                        if (score > bestScore)
                        {
                            bestScore = score;
                            backwardExtension = i;
                        }
                    }
                }

                int forwardExtension = 0;
                {
                    int score = 0;
                    int bestScore = 0;
                    await oldData.SeekAsync(lastOldPosition);
                    await newData.SeekAsync(lastNewPosition);
                    for (int i = 0; lastNewPosition + i < newPosition && lastOldPosition + i < oldData.Length; ++i)
                    {
                        if (await oldData.ReadByteAsync() == await newData.ReadByteAsync())
                        {
                            ++score;
                        }
                        else
                        {
                            --score;
                        }
                        if (score > bestScore)
                        {
                            bestScore = score;
                            forwardExtension = i + 1;
                        }
                    }
                }

                int overlap = (lastNewPosition + forwardExtension) - (newPosition - backwardExtension);
                if (overlap > 0)
                {
                    int score = 0;
                    int bestScore = 0;
                    int backwardExtensionDecrement = 0;
                    for (int i = 0; i < overlap; ++i)
                    {
                        await newData.SeekAsync(lastNewPosition + forwardExtension - overlap + i);
                        await oldData.SeekAsync(lastOldPosition + forwardExtension - overlap + i);
                        if (await newData.ReadByteAsync() == await oldData.ReadByteAsync())
                        {
                            ++score;
                        }

                        await newData.SeekAsync(newPosition - backwardExtension + i);
                        await oldData.SeekAsync(oldPosition - backwardExtension + i);
                        if (await newData.ReadByteAsync() == await oldData.ReadByteAsync())
                        {
                            --score;
                        }
                        if (score > bestScore)
                        {
                            bestScore = score;
                            backwardExtensionDecrement = i + 1;
                        }
                    }
                    forwardExtension -= overlap - backwardExtensionDecrement;
                    backwardExtension -= backwardExtensionDecrement;
                }

                int oldPositionOffset = 0;
                if (newPosition < newData.Length)
                {
                    oldPositionOffset = (oldPosition - backwardExtension) - (lastOldPosition + forwardExtension);
                }

                int newNoMatchLength = (newPosition - backwardExtension) - (lastNewPosition + forwardExtension);

                await WriteEntryAsync(newData, oldData, lastNewPosition, lastOldPosition, forwardExtension, newNoMatchLength, oldPositionOffset, outputStream);

                lastNewPosition = newPosition - backwardExtension;
                lastOldPosition = oldPosition - backwardExtension;
            }
        }

        public static async Task GeneratePatchAsync(
            IRandomAccessObject oldData,
            IRandomAccessObject newData,
            Stream outputStream,
            IRandomAccessObjectFactory randomAccessObjectFactory)
        {
            await GeneratePatchAsync(oldData, newData, outputStream, randomAccessObjectFactory, DefaultMinimumMatchLength);
        }

        public static async Task GeneratePatchAsync(
            byte[] oldData,
            byte[] newData,
            Stream outputStream)
        {
            await GeneratePatchAsync(oldData, newData, outputStream, DefaultMinimumMatchLength);
        }

        public static async Task GeneratePatchAsync(
            byte[] oldData,
            byte[] newData,
            Stream outputStream,
            int minimumMatchLength)
        {
            using (var oldDataRAO = new RandomAccessByteArrayObject(oldData))
            using (var newDataRAO = new RandomAccessByteArrayObject(newData))
            {
                await GeneratePatchAsync(oldDataRAO, newDataRAO, outputStream, new RandomAccessByteArrayObjectFactory(), minimumMatchLength);
            }
        }

        public static async Task GeneratePatchAsync(
            FileInfo oldData,
            FileInfo newData,
            Stream outputStream)
        {
            await GeneratePatchAsync(oldData, newData, outputStream, DefaultMinimumMatchLength);
        }

        public static async Task GeneratePatchAsync(
            FileInfo oldData,
            FileInfo newData,
            Stream outputStream,
            int minimumMatchLength)
        {
            using (var oldDataRAF = new RandomAccessFileObject(oldData, FileAccess.Read))
            using (var newDataRAF = new RandomAccessFileObject(newData, FileAccess.Read))
            {
                await GeneratePatchAsync(
                    oldDataRAF,
                    newDataRAF,
                    outputStream,
                    new RandomAccessMmapObjectFactory(FileAccess.ReadWrite),
                    minimumMatchLength);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static async Task GeneratePatchAsync(
            IRandomAccessObject oldData,
            IRandomAccessObject newData,
            Stream outputStream,
            IRandomAccessObjectFactory randomAccessObjectFactory,
            int minimumMatchLength)
        {
            // Write header (signature + new file length)
            await outputStream.WriteAsync(Encoding.ASCII.GetBytes("ENDSLEY/BSDIFF43"));
            await BsUtil.WriteFormattedLongAsync(newData.Length, outputStream);

            // Do the suffix search.
            using (var groupArray = await new DivSuffixSorter(randomAccessObjectFactory).SuffixSortAsync(oldData))
            {
                var matcher = new BsDiffMatcher(oldData, newData, groupArray, minimumMatchLength);
                await GeneratePatchWithMatcherAsync(oldData, newData, matcher, outputStream);
            }
        }
    }
}