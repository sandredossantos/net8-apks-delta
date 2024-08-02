namespace Delta.Domain.Generator.shared
{
    public class RandomAccessFileInputStream : Stream
    {
        private readonly FileStream _fileStream;
        private long _mark = -1;
        private long _rangeOffset;
        private long _rangeLength;
        private readonly long _fileLength;

        public RandomAccessFileInputStream(string filePath)
            : this(filePath, 0, new FileInfo(filePath).Length)
        {
        }

        public RandomAccessFileInputStream(string filePath, long rangeOffset, long rangeLength)
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _fileLength = _fileStream.Length;
            SetRange(rangeOffset, rangeLength);
        }

        private void SetRange(long rangeOffset, long rangeLength)
        {
            if (rangeOffset < 0)
                throw new ArgumentException("rangeOffset must be >= 0");
            if (rangeLength < 0)
                throw new ArgumentException("rangeLength must be >= 0");
            if (rangeOffset + rangeLength > _fileLength)
                throw new ArgumentException("Read range exceeds file length");
            if (rangeOffset + rangeLength < 0)
                throw new ArgumentException("Insane input size not supported");

            _rangeOffset = rangeOffset;
            _rangeLength = rangeLength;
            _mark = rangeOffset;
            _fileStream.Seek(_rangeOffset, SeekOrigin.Begin);
            _mark = -1;
        }

        public override bool CanRead => _fileStream.CanRead;

        public override bool CanSeek => _fileStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _fileLength;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _fileStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            _fileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
                return 0;

            int available = (int)Available();
            if (available <= 0)
                return -1;

            int result = _fileStream.Read(buffer, offset, (int)Math.Min(count, available));
            return result;
        }

        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        public long Skip(long count)
        {
            if (count <= 0)
                return 0;

            int available = (int)Available();
            if (available <= 0)
                return 0;

            long skipAmount = Math.Min(available, count);
            _fileStream.Seek(skipAmount, SeekOrigin.Current);
            return skipAmount;
        }

        public override void Close()
        {
            _fileStream.Close();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override bool CanTimeout => false;

        public override int ReadTimeout
        {
            get => _fileStream.ReadTimeout;
            set => _fileStream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _fileStream.WriteTimeout;
            set => _fileStream.WriteTimeout = value;
        }

        public override long Position
        {
            get => _fileStream.Position;
            set => _fileStream.Seek(value, SeekOrigin.Begin);
        }

        public void Mark(int readlimit)
        {
            try
            {
                _mark = _fileStream.Position;
            }
            catch (IOException e)
            {
                throw new InvalidOperationException(e.Message, e);
            }
        }

        public void Reset()
        {
            if (_mark < 0)
                throw new IOException("mark not set");

            _fileStream.Seek(_mark, SeekOrigin.Begin);
        }

        private long Available()
        {
            long rangeRelativePosition = _fileStream.Position - _rangeOffset;
            long result = _rangeLength - rangeRelativePosition;
            return result > int.MaxValue ? int.MaxValue : result;
        }
    }
}
