namespace Delta.Domain.Generator
{
    public enum RecommendationReason : byte
    {
        DEFLATE_UNSUITABLE = 1,
        UNSUITABLE = 2,
        BOTH_ENTRIES_UNCOMPRESSED = 3,
        UNCOMPRESSED_CHANGED_TO_COMPRESSED = 4,
        COMPRESSED_CHANGED_TO_UNCOMPRESSED = 5,
        COMPRESSED_BYTES_CHANGED = 6,
        COMPRESSED_BYTES_IDENTICAL = 7,
        RESOURCE_CONSTRAINED = 9
    }
}