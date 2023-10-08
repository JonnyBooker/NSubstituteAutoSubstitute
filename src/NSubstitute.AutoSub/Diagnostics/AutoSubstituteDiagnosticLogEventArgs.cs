using System;

namespace NSubstitute.AutoSub.Diagnostics;

/// <summary>
/// Events Arguments for when a diagnostic log is created
/// </summary>
public class AutoSubstituteDiagnosticLogEventArgs : EventArgs
{
    /// <summary>
    /// The Type being checked/operated on when creating a constructor argument
    /// </summary>
    public Type Type { get; }
    
    /// <summary>
    /// The detailed information for the type being created
    /// </summary>
    public string Message { get; }

    internal AutoSubstituteDiagnosticLogEventArgs(Type type, string message)
    {
        Type = type;
        Message = message;
    }
}