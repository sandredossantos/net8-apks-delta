namespace Delta.Domain.Generator
{
    public class MatchingOutputStream : Stream
    {
        private readonly Stream _expectedBytesStream;
        private readonly byte[] _buffer;

        public MatchingOutputStream(Stream expectedBytesStream, int matchBufferSize)
        {
            if (matchBufferSize < 1)
            {
                throw new ArgumentException("Buffer size must be >= 1");
            }
            _expectedBytesStream = expectedBytesStream;
            _buffer = new byte[matchBufferSize];
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int numReadSoFar = 0;
            while (numReadSoFar < count)
            {
                int maxToRead = Math.Min(_buffer.Length, count - numReadSoFar);
                int numReadThisLoop = _expectedBytesStream.Read(_buffer, 0, maxToRead);
                if (numReadThisLoop == -1)
                {
                    throw new MismatchException("EOF reached in expectedBytesStream");
                }
                for (int matchCount = 0; matchCount < numReadThisLoop; matchCount++)
                {
                    if (_buffer[matchCount] != buffer[offset + numReadSoFar + matchCount])
                    {
                        throw new MismatchException("Data does not match");
                    }
                }
                numReadSoFar += numReadThisLoop;
            }
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public void Write(int value)
        {
            int expected = _expectedBytesStream.ReadByte();
            if (expected == -1)
            {
                throw new MismatchException("EOF reached in expectedBytesStream");
            }
            if (expected != value)
            {
                throw new MismatchException("Data does not match");
            }
        }

        public override void Close()
        {
            _expectedBytesStream.Close();
        }

        public void ExpectEof()
        {
            if (_expectedBytesStream.ReadByte() != -1)
            {
                throw new MismatchException("EOF not reached in expectedBytesStream");
            }
        }

        public override bool CanRead => _expectedBytesStream.CanRead;
        public override bool CanSeek => _expectedBytesStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _expectedBytesStream.Length;
        public override long Position
        {
            get => _expectedBytesStream.Position;
            set => _expectedBytesStream.Position = value;
        }
        public override long Seek(long offset, SeekOrigin origin) => _expectedBytesStream.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Flush() => _expectedBytesStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
