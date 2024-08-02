namespace Delta.Domain.Generator.bsdiff
{
    public interface ISuffixSorter
    {
        Task<RandomAccessObject> SuffixSortAsync(IRandomAccessObject data);
    }
}