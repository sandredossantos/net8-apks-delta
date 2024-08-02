namespace Delta.Domain.Generator.bsdiff
{
    class BsDiffMatcher : IMatcher
    {
        private readonly RandomAccessObject _oldData;
        private readonly RandomAccessObject _newData;
        
        /// <summary>
        /// Contains order of the sorted suffixes of |oldData|. The element at mGroupArray[i] contains the
        /// position of oldData[i ... oldData.length - 1] in the sorted list of suffixes of |oldData|.
        /// </summary>
        private readonly RandomAccessObject _groupArray;

        /// <summary>
        /// The index in |oldData| of the first byte of the match. Zero if no matches have been found yet.
        /// </summary>
        private int _oldPos;

        /// <summary>
        /// The index in |newData| of the first byte of the match. Zero if no matches have been found yet.
        /// The next match will be searched starting at |mNewPos| + |mMatchLen|.
        /// </summary>
        private int _newPos;

        /// <summary>
        /// Minimum match length in bytes.
        /// </summary>
        private readonly int _minimumMatchLength;

        /// <summary>
        /// A limit on how many total match lengths encountered, to exit the match extension loop in next()
        /// and prevent O(n^2) behavior.
        /// </summary>
        private readonly long _totalMatchLenBudget = 1L << 26;  // ~64 million.

        /// <summary>
        /// The number of bytes, |n|, which match between newData[mNewPos ... mNewPos + n] and
        /// oldData[mOldPos ... mOldPos + n].
        /// </summary>
        private int _matchLen;

        /// <summary>
        /// Create a standard BsDiffMatcher.
        /// </summary>
        /// <param name="minimumMatchLength">the minimum "match" (in bytes) for BsDiff to consider between the
        /// oldData and newData. This can have a significant effect on both the generated patch size and
        /// the amount of time and memory required to apply the patch.</param>
        public BsDiffMatcher(RandomAccessObject oldData, RandomAccessObject newData, RandomAccessObject groupArray, int minimumMatchLength)
        {
            _oldData = oldData;
            _newData = newData;
            _groupArray = groupArray;
            _oldPos = 0;
            _minimumMatchLength = minimumMatchLength;
        }

        public Matcher.NextMatch Next()
        {
            RandomAccessObject oldData = _oldData;
            RandomAccessObject newData = _newData;

            // The offset between between the indices in |oldData| and |newData|
            // of the previous match.
            int previousOldOffset = _oldPos - _newPos;

            // Look for a new match starting from the end of the previous match.
            _newPos += _matchLen;

            // The number of matching bytes in the forward extension of the previous match:
            // oldData[mNewPos + previousOldOffset ... mNewPos + previousOldOffset + mMatchLen - 1]
            // and newData[mNewPos ... mNewPos + mMatchLen - 1].
            int numMatches = 0;

            // The size of the range for which |numMatches| has been computed.
            int matchesCacheSize = 0;

            // Sum over all match lengths encountered, to exit loop if we take too long to compute.
            long totalMatchLen = 0;

            while (_newPos < newData.Length)
            {
                if (Thread.CurrentThread.IsInterrupted)
                {
                    throw new ThreadInterruptedException();
                }
                BsDiff.Match match = BsDiff.SearchForMatch(_groupArray, oldData, newData, _newPos, 0, (int)oldData.Length);
                _oldPos = match.Start;
                _matchLen = match.Length;
                totalMatchLen += _matchLen;

                // Update |numMatches| for the new value of |matchLen|.
                for (; matchesCacheSize < _matchLen; ++matchesCacheSize)
                {
                    int oldIndex = _newPos + previousOldOffset + matchesCacheSize;
                    int newIndex = _newPos + matchesCacheSize;
                    if (oldIndex < oldData.Length)
                    {
                        oldData.Seek(oldIndex);
                        newData.Seek(newIndex);

                        if (oldData.ReadByte() == newData.ReadByte())
                        {
                            ++numMatches;
                        }
                    }
                }

                // Also return if we've been trying to extend a large match for a long time.
                if (_matchLen > numMatches + _minimumMatchLength || totalMatchLen >= _totalMatchLenBudget)
                {
                    return Matcher.NextMatch.Of(true, _oldPos, _newPos);
                }

                if (_matchLen == 0)
                {
                    ++_newPos;
                }
                else if (_matchLen == numMatches)
                {
                    // This seems to be an optimization because it is unlikely to find a valid match in the
                    // range mNewPos = [ mNewPos ... mNewPos + mMatchLen - 1 ] especially for large values of
                    // |mMatchLen|.
                    _newPos += numMatches;
                    numMatches = 0;
                    matchesCacheSize = 0;
                }
                else
                {
                    // Update |numMatches| for the value of |mNewPos| in the next iteration of the loop. In the
                    // next iteration of the loop, the new value of |numMatches| will be at least
                    // |numMatches - 1| because
                    // oldData[mNewPos + previousOldOffset + 1 ... mNewPos + previousOldOffset + mMatchLen - 1]
                    // matches newData[mNewPos + 1 ... mNewPos + mMatchLen - 1].
                    if (_newPos + previousOldOffset < oldData.Length)
                    {
                        oldData.Seek(_newPos + previousOldOffset);
                        newData.Seek(_newPos);

                        if (oldData.ReadByte() == newData.ReadByte())
                        {
                            --numMatches;
                        }
                    }
                    ++_newPos;
                    --matchesCacheSize;
                }
            }

            return Matcher.NextMatch.Of(false, 0, 0);
        }
    }
}