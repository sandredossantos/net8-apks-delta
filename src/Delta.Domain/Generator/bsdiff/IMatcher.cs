namespace Delta.Domain.Generator.bsdiff
{
    public interface IMatcher
    {
        NextMatch Next();
    }

    public class NextMatch
    {
        public bool DidFindMatch { get; }
        public int OldPosition { get; }
        public int NewPosition { get; }

        private NextMatch(bool didFindMatch, int oldPosition, int newPosition)
        {
            DidFindMatch = didFindMatch;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public static NextMatch Of(bool didFindMatch, int oldPosition, int newPosition)
        {
            return new NextMatch(didFindMatch, oldPosition, newPosition);
        }
    }
}
