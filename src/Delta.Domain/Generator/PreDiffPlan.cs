using Delta.Domain.Generator.shared;

namespace Delta.Domain.Generator
{
    public class PreDiffPlan
    {
        private readonly IList<TypedRange<object>> _oldFileUncompressionPlan;

        private readonly IList<TypedRange<JreDeflateParameters>> _newFileUncompressionPlan;

        private readonly IList<TypedRange<JreDeflateParameters>> _deltaFriendlyNewFileRecompressionPlan;

        private readonly IList<QualifiedRecommendation> _qualifiedRecommendations;

        public PreDiffPlan(
            IList<QualifiedRecommendation> qualifiedRecommendations,
            IList<TypedRange<object>> oldFileUncompressionPlan,
            IList<TypedRange<JreDeflateParameters>> newFileUncompressionPlan)
            : this(qualifiedRecommendations, oldFileUncompressionPlan, newFileUncompressionPlan, null) { }

        public PreDiffPlan(
            IList<QualifiedRecommendation> qualifiedRecommendations,
            IList<TypedRange<object>> oldFileUncompressionPlan,
            IList<TypedRange<JreDeflateParameters>> newFileUncompressionPlan,
            IList<TypedRange<JreDeflateParameters>> deltaFriendlyNewFileRecompressionPlan)
        {
            EnsureOrdered(oldFileUncompressionPlan);
            EnsureOrdered(newFileUncompressionPlan);
            EnsureOrdered(deltaFriendlyNewFileRecompressionPlan);
            _qualifiedRecommendations = qualifiedRecommendations;
            _oldFileUncompressionPlan = oldFileUncompressionPlan;
            _newFileUncompressionPlan = newFileUncompressionPlan;
            _deltaFriendlyNewFileRecompressionPlan = deltaFriendlyNewFileRecompressionPlan;
        }

        private static void EnsureOrdered<T>(IList<TypedRange<T>> list)
        {
            if (list != null && list.Count >= 2)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    if (list[i].CompareTo(list[i + 1]) > 0)
                    {
                        throw new ArgumentException("List must be ordered");
                    }
                }
            }
        }

        public IList<TypedRange<object>> GetOldFileUncompressionPlan()
        {
            return _oldFileUncompressionPlan;
        }

        public IList<TypedRange<JreDeflateParameters>> GetNewFileUncompressionPlan()
        {
            return _newFileUncompressionPlan;
        }

        public IList<TypedRange<JreDeflateParameters>> GetDeltaFriendlyNewFileRecompressionPlan()
        {
            return _deltaFriendlyNewFileRecompressionPlan;
        }

        public IList<QualifiedRecommendation> GetQualifiedRecommendations()
        {
            return _qualifiedRecommendations;
        }
    }
}
