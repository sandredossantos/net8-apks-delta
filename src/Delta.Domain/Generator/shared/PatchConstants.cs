namespace Delta.Domain.Generator.shared
{
    public static class PatchConstants
    {
        public const string IDENTIFIER = "GFbFv1_0";

        public enum CompatibilityWindowId
        {
            DEFAULT_DEFLATE = 0
        }

        public enum DeltaFormat
        {
            BSDIFF = 0
        }

        public static CompatibilityWindowId? GetCompatibilityWindowId(byte patchValue)
        {
            return patchValue switch
            {
                0 => CompatibilityWindowId.DEFAULT_DEFLATE,
                _ => null
            };
        }

        public static DeltaFormat? GetDeltaFormat(byte patchValue)
        {
            return patchValue switch
            {
                0 => DeltaFormat.BSDIFF,
                _ => null
            };
        }
    }
}
