namespace Delta.Domain.Generator.Bsdiff
{
    public interface IRandomAccessObject : IDisposable
    {
        long Length { get; }
        void Seek(long pos);
        void SeekToIntAligned(long pos);
    }

    public class RandomAccessFileObject : IRandomAccessObject
    {
        private readonly bool _shouldDeleteFileOnClose;
        private readonly FileStream _fileStream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;
        private string _tempFileName;

        public RandomAccessFileObject(FileInfo tempFile, FileMode mode, bool deleteFileOnClose = false)
        {
            _shouldDeleteFileOnClose = deleteFileOnClose;
            _fileStream = new FileStream(tempFile.FullName, mode, FileAccess.ReadWrite);
            _reader = new BinaryReader(_fileStream);
            _writer = new BinaryWriter(_fileStream);

            if (_shouldDeleteFileOnClose)
            {
                _tempFileName = tempFile.FullName;
            }
        }

        public long Length => _fileStream.Length;

        public void Seek(long pos)
        {
            _fileStream.Seek(pos, SeekOrigin.Begin);
        }

        public void SeekToIntAligned(long pos)
        {
            Seek(pos * 4);
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _fileStream?.Dispose();

            if (_shouldDeleteFileOnClose && !string.IsNullOrEmpty(_tempFileName))
            {
                try
                {
                    File.Delete(_tempFileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file: {ex.Message}");
                }
            }
        }
    }

    public class RandomAccessByteArrayObject : IRandomAccessObject
    {
        protected byte[] _byteArray;
        private int _position;

        public RandomAccessByteArrayObject(byte[] byteArray)
        {
            _byteArray = byteArray;
        }

        public RandomAccessByteArrayObject(int length)
        {
            _byteArray = new byte[length];
        }

        public long Length => _byteArray.Length;

        public void Seek(long pos)
        {
            if (pos > int.MaxValue)
            {
                throw new ArgumentException("RandomAccessByteArrayObject can only handle seek() addresses up to Int32.MaxValue.");
            }
            _position = (int)pos;
        }

        public void SeekToIntAligned(long pos)
        {
            Seek(pos * 4);
        }

        public byte ReadByte()
        {
            return _byteArray[_position++];
        }

        public int ReadInt()
        {
            int result = BitConverter.ToInt32(_byteArray, _position);
            _position += 4;
            return result;
        }

        public void WriteByte(byte b)
        {
            _byteArray[_position++] = b;
        }

        public void WriteInt(int i)
        {
            BitConverter.GetBytes(i).CopyTo(_byteArray, _position);
            _position += 4;
        }

        public void Dispose()
        {
            // No-op
        }

        // Implement other methods (e.g., ReadBoolean, ReadChar, WriteChar, etc.) similarly
    }

    public class RandomAccessMmapObject : RandomAccessByteArrayObject
    {
        private readonly FileStream _fileStream;
        private readonly FileInfo _fileInfo;
        private readonly bool _shouldDeleteFileOnRelease;

        public RandomAccessMmapObject(string tempFileName, FileMode mode, long length)
            : base((int)length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentException("RandomAccessMmapObject only supports file sizes up to Int32.MaxValue.");
            }

            _fileInfo = new FileInfo(Path.GetTempFileName());
            _shouldDeleteFileOnRelease = true;

            _fileStream = new FileStream(_fileInfo.FullName, mode, FileAccess.ReadWrite);
            _fileStream.SetLength(length);

            // Map the file into memory
            _fileStream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[length];
            _fileStream.Read(buffer, 0, (int)length);
            _byteArray = buffer;
        }

        public RandomAccessMmapObject(FileInfo tempFile, FileMode mode)
            : base((int)tempFile.Length)
        {
            if (tempFile.Length > int.MaxValue)
            {
                throw new ArgumentException("Only files up to 2GiB in size are supported.");
            }

            _fileInfo = tempFile;
            _shouldDeleteFileOnRelease = true;

            _fileStream = new FileStream(_fileInfo.FullName, mode, FileAccess.ReadWrite);

            // Map the file into memory
            var buffer = new byte[tempFile.Length];
            _fileStream.Read(buffer, 0, (int)tempFile.Length);
            _byteArray = buffer;
        }

        public override void Dispose()
        {
            base.Dispose();
            _fileStream?.Dispose();

            if (_shouldDeleteFileOnRelease && _fileInfo != null && File.Exists(_fileInfo.FullName))
            {
                try
                {
                    File.Delete(_fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file: {ex.Message}");
                }
            }
        }
    }
}
