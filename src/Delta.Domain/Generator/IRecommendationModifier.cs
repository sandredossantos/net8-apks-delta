namespace Delta.Domain.Generator
{
    public interface IRecommendationModifier
    {
        List<QualifiedRecommendation> GetModifiedRecommendations(FileInfo oldFile, FileInfo newFile, List<QualifiedRecommendation> originalRecommendations);
    }
}