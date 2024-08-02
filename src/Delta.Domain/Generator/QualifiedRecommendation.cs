namespace Delta.Domain.Generator
{
    public class QualifiedRecommendation
    {
        public MinimalZipEntry OldEntry { get; set; }
        public MinimalZipEntry NewEntry { get; set; }
        public Recommendation Recommendation { get; set; }
        public RecommendationReason Reason { get; set; }

        public QualifiedRecommendation(
            MinimalZipEntry oldEntry,
            MinimalZipEntry newEntry,
            Recommendation recommendation,
            RecommendationReason reason)
        {
            OldEntry = oldEntry;
            NewEntry = newEntry;
            Recommendation = recommendation;
            Reason = reason;
        }

        public MinimalZipEntry GetOldEntry() => OldEntry;

        public MinimalZipEntry GetNewEntry() => NewEntry;

        public Recommendation GetRecommendation() => Recommendation;

        public RecommendationReason GetRecommendationReason() => Reason;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (QualifiedRecommendation)obj;
            return Equals(NewEntry, other.NewEntry) &&
                   Equals(OldEntry, other.OldEntry) &&
                   Recommendation == other.Recommendation &&
                   Reason == other.Reason;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = OldEntry?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (NewEntry?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Recommendation.GetHashCode();
                hashCode = (hashCode * 397) ^ Reason.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"QualifiedRecommendation [oldEntry={OldEntry?.GetFileName()}, newEntry={NewEntry?.GetFileName()}, recommendation={Recommendation}, reason={Reason}]";
        }
    }
}