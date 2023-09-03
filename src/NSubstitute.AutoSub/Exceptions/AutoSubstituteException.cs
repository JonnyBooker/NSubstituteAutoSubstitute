using System;

namespace NSubstitute.AutoSub.Exceptions;

/// <summary>
/// Exception that is found internally within AutoSubstitute
/// </summary>
public class AutoSubstituteException : Exception
{
    /// <summary>
    /// Default constructor that expects a message
    /// </summary>
    /// <param name="message">Error Message</param>
    public AutoSubstituteException(string message) : base(message)
    {
    }
}