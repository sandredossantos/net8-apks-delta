namespace Delta.Domain.Generator
{
    public class ByteArrayHolder
    {
        private readonly byte[] data;

        public ByteArrayHolder(byte[] data)
        {
            this.data = data;
        }

        public byte[] GetData()
        {
            return data;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + data.Aggregate(0, (acc, b) => acc * 31 + b);
            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            ByteArrayHolder other = (ByteArrayHolder)obj;
            return data.SequenceEqual(other.data);
        }
    }
}