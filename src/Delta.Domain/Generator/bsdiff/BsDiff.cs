namespace Delta.Domain.Generator.bsdiff
{
    public static class BsDiff
    {
        /// <summary>
        /// Search the specified arrays for a contiguous sequence of identical bytes, starting at the
        /// specified "start" offsets and scanning as far ahead as possible till one or the other of the
        /// arrays ends or a non-matching byte is found. Returns the length of the matching sequence of
        /// bytes, which may be zero.
        /// </summary>
        /// <param name="oldData">The old data to scan</param>
        /// <param name="oldStart">The position in the old data at which to start the scan</param>
        /// <param name="newData">The new data to scan</param>
        /// <param name="newStart">The position in the new data at which to start the scan</param>
        /// <returns>The number of matching bytes in the two arrays starting at the specified indices; zero if the first byte fails to match</returns>
        public static int LengthOfMatch(Stream oldData, int oldStart, Stream newData, int newStart)
        {
            int max = (int)Math.Min(oldData.Length - oldStart, newData.Length - newStart);
            if (max > 0)
            {
                oldData.Seek(oldStart, SeekOrigin.Begin);
                newData.Seek(newStart, SeekOrigin.Begin);
                for (int offset = 0; offset < max; offset++)
                {
                    if (oldData.ReadByte() != newData.ReadByte())
                    {
                        return offset;
                    }
                }
            }

            return max;
        }

        public static Match SearchForMatchBaseCase(Stream groupArray, Stream oldData, Stream newData, int newStart, int oldDataRangeStartA, int oldDataRangeStartB)
        {
            groupArray.Seek(oldDataRangeStartA * 4, SeekOrigin.Begin);
            int groupArrayOldDataRangeStartA = ReadInt(groupArray);
            int lengthOfMatchA = LengthOfMatch(oldData, groupArrayOldDataRangeStartA, newData, newStart);
            groupArray.Seek(oldDataRangeStartB * 4, SeekOrigin.Begin);
            int groupArrayOldDataRangeStartB = ReadInt(groupArray);
            int lengthOfMatchB = LengthOfMatch(oldData, groupArrayOldDataRangeStartB, newData, newStart);

            if (lengthOfMatchA > lengthOfMatchB)
            {
                return Match.Of(groupArrayOldDataRangeStartA, lengthOfMatchA);
            }

            return Match.Of(groupArrayOldDataRangeStartB, lengthOfMatchB);
        }

        /// <summary>
        /// Locates the run of bytes in |oldData| which matches the longest prefix of newData[newStart ... newData.Length - 1].
        /// </summary>
        /// <param name="groupArray"></param>
        /// <param name="oldData">The old data to scan</param>
        /// <param name="newData">The new data to scan</param>
        /// <param name="newStart">The position of the first byte in newData to consider</param>
        /// <param name="oldDataRangeStartA"></param>
        /// <param name="oldDataRangeStartB"></param>
        /// <returns>A Match containing the length of the matching range, and the position at which the matching range begins.</returns>
        public static Match SearchForMatch(Stream groupArray, Stream oldData, Stream newData, int newStart, int oldDataRangeStartA, int oldDataRangeStartB)
        {
            if (oldDataRangeStartB - oldDataRangeStartA < 2)
            {
                return SearchForMatchBaseCase(groupArray, oldData, newData, newStart, oldDataRangeStartA, oldDataRangeStartB);
            }

            // Cut range in half and search again
            int rangeLength = oldDataRangeStartB - oldDataRangeStartA;
            int pivot = oldDataRangeStartA + (rangeLength / 2);
            groupArray.Seek(pivot * 4, SeekOrigin.Begin);
            int groupArrayPivot = ReadInt(groupArray);
            if (BsUtil.LexicographicalCompare(oldData, groupArrayPivot, (int)oldData.Length - groupArrayPivot, newData, newStart, (int)newData.Length - newStart) < 0)
            {
                return SearchForMatch(groupArray, oldData, newData, newStart, pivot, oldDataRangeStartB);
            }
            return SearchForMatch(groupArray, oldData, newData, newStart, oldDataRangeStartA, pivot);
        }

        public class Match
        {
            public int Start { get; }
            public int Length { get; }

            public static Match Of(int start, int length)
            {
                return new Match(start, length);
            }

            private Match(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }

        private static int ReadInt(Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}