using System;

namespace NSubstitute.AutoSub.Exceptions;

public class AutoSubstituteException : Exception
{
    public AutoSubstituteException(string message) : base(message)
    {
    }
}