namespace Delta.Domain.Generator.bsdiff
{
    public class BsUtil
    {
        /// <summary>
        /// Mask to determine whether a long written by <see cref="WriteFormattedLong(long, Stream)"/>
        /// is negative.
        /// </summary>
        private static readonly long NEGATIVE_MASK = 1L << 63;

        /// <summary>
        /// Writes a 64-bit signed integer to the specified <see cref="Stream"/>. The least significant
        /// byte is written first and the most significant byte is written last.
        /// </summary>
        /// <param name="value">the value to write</param>
        /// <param name="outputStream">the stream to write to</param>
        public static void WriteFormattedLong(long value, Stream outputStream)
        {
            long y = value;
            if (y < 0)
            {
                y = (-y) | NEGATIVE_MASK;
            }

            for (int i = 0; i < 8; ++i)
            {
                outputStream.WriteByte((byte)(y & 0xff));
                y >>= 8;
            }
        }

        /// <summary>
        /// Reads a 64-bit signed integer written by <see cref="WriteFormattedLong(long, Stream)"/> from
        /// the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="inputStream">the stream to read from</param>
        public static long ReadFormattedLong(Stream inputStream)
        {
            long result = 0;
            for (int bitshift = 0; bitshift < 64; bitshift += 8)
            {
                result |= ((long)inputStream.ReadByte()) << bitshift;
            }

            if ((result - NEGATIVE_MASK) > 0)
            {
                result = (result & ~NEGATIVE_MASK) * -1;
            }
            return result;
        }

        /// <summary>
        /// Provides functional equivalent to C/C++ lexicographical_compare. Warning: this calls <see cref="RandomAccessObject.Seek(long)"/>,
        /// so the internal state of the data objects will be modified.
        /// </summary>
        /// <param name="data1">first byte array</param>
        /// <param name="start1">index in the first array at which to start comparing</param>
        /// <param name="length1">length of first byte array</param>
        /// <param name="data2">second byte array</param>
        /// <param name="start2">index in the second array at which to start comparing</param>
        /// <param name="length2">length of second byte array</param>
        /// <returns>
        /// result of lexicographical compare: negative if the first difference has a lower value
        /// in the first array, positive if the first difference has a lower value in the second array.
        /// If both arrays compare equal until one of them ends, the shorter sequence is
        /// lexicographically less than the longer one (i.e., it returns len(first array) -
        /// len(second array)).
        /// </returns>
        public static int LexicographicalCompare(
            RandomAccessObject data1,
            int start1,
            int length1,
            RandomAccessObject data2,
            int start2,
            int length2)
        {
            int bytesLeft = Math.Min(length1, length2);

            data1.Seek(start1);
            data2.Seek(start2);
            while (bytesLeft-- > 0)
            {
                int i1 = data1.ReadUnsignedByte();
                int i2 = data2.ReadUnsignedByte();

                if (i1 != i2)
                {
                    return i1 - i2;
                }
            }

            return length1 - length2;
        }
    }
}
