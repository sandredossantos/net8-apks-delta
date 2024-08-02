using System;

namespace Delta.Domain.Generator
{
    public class MismatchException : IOException
    {
        public MismatchException(string message) : base(message) { }
    }
}
