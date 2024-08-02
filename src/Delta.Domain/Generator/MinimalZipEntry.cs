using System.Text;

namespace Delta.Domain.Generator
{
    public class MinimalZipEntry
    {
        public int CompressionMethod { get; }

        public long Crc32OfUncompressedData { get; }

        public long CompressedSize { get; }

        public long UncompressedSize { get; }

        private readonly byte[] fileNameBytes;

        public bool GeneralPurposeFlagBit11 { get; }

        public long FileOffsetOfLocalEntry { get; }

        public long FileOffsetOfCompressedData { get; private set; } = -1;
      
        public MinimalZipEntry(
            int compressionMethod,
            long crc32OfUncompressedData,
            long compressedSize,
            long uncompressedSize,
            byte[] fileNameBytes,
            bool generalPurposeFlagBit11,
            long fileOffsetOfLocalEntry)
        {
            CompressionMethod = compressionMethod;
            Crc32OfUncompressedData = crc32OfUncompressedData;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
            this.fileNameBytes = fileNameBytes?.Clone() as byte[];
            GeneralPurposeFlagBit11 = generalPurposeFlagBit11;
            FileOffsetOfLocalEntry = fileOffsetOfLocalEntry;
        }

        public byte[]? GetFileNameBytes()
        {
            return fileNameBytes == null ? null : (byte[])fileNameBytes.Clone();
        }

        public void SetFileOffsetOfCompressedData(long offset)
        {
            FileOffsetOfCompressedData = offset;
        }

        public string GetFileName()
        {
            string charsetName = GeneralPurposeFlagBit11 ? "UTF-8" : "IBM437";
            try
            {
                return Encoding.GetEncoding(charsetName).GetString(fileNameBytes);
            }
            catch (EncoderFallbackException e)
            {
                throw new InvalidOperationException("System doesn't support " + charsetName, e);
            }
        }

        public bool GetGeneralPurposeFlagBit11() => GeneralPurposeFlagBit11;

        public long GetFileOffsetOfLocalEntry() => FileOffsetOfLocalEntry;

        public long GetFileOffsetOfCompressedData() => FileOffsetOfCompressedData;

        public bool IsDeflateCompressed()
        {
            if (CompressionMethod != 8) return false;

            return CompressedSize != UncompressedSize;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (MinimalZipEntry)obj;
            return CompressedSize == other.CompressedSize &&
                   CompressionMethod == other.CompressionMethod &&
                   Crc32OfUncompressedData == other.Crc32OfUncompressedData &&
                   Equals(fileNameBytes, other.fileNameBytes) &&
                   FileOffsetOfCompressedData == other.FileOffsetOfCompressedData &&
                   FileOffsetOfLocalEntry == other.FileOffsetOfLocalEntry &&
                   GeneralPurposeFlagBit11 == other.GeneralPurposeFlagBit11 &&
                   UncompressedSize == other.UncompressedSize;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CompressionMethod;
                hashCode = (hashCode * 397) ^ (int)(Crc32OfUncompressedData ^ (Crc32OfUncompressedData >> 32));
                hashCode = (hashCode * 397) ^ (int)(CompressedSize ^ (CompressedSize >> 32));
                hashCode = (hashCode * 397) ^ (int)(UncompressedSize ^ (UncompressedSize >> 32));
                hashCode = (hashCode * 397) ^ (fileNameBytes != null ? BitConverter.ToInt32(fileNameBytes, 0) : 0);
                hashCode = (hashCode * 397) ^ (int)(FileOffsetOfCompressedData ^ (FileOffsetOfCompressedData >> 32));
                hashCode = (hashCode * 397) ^ (int)(FileOffsetOfLocalEntry ^ (FileOffsetOfLocalEntry >> 32));
                hashCode = (hashCode * 397) ^ (GeneralPurposeFlagBit11 ? 1231 : 1237);
                return hashCode;
            }
        }
    }
}