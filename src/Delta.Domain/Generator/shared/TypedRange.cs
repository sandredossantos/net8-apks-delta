using System;

namespace Delta.Domain.Generator.shared
{
    public class TypedRange<T> : IComparable<TypedRange<T>>
    {
        public long Offset { get; }
        public long Length { get; }
        public T Metadata { get; }

        public TypedRange(long offset, long length, T metadata)
        {
            Offset = offset;
            Length = length;
            Metadata = metadata;
        }

        public override string ToString()
        {
            return $"offset {Offset}, length {Length}, metadata {Metadata}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is TypedRange<T> other)
            {
                return Offset == other.Offset &&
                       Length == other.Length &&
                       Equals(Metadata, other.Metadata);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Offset.GetHashCode();
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                hashCode = (hashCode * 397) ^ (Metadata?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public int CompareTo(TypedRange<T> other)
        {
            if (other == null) return 1;
            return Offset.CompareTo(other.Offset);
        }
    }
}

