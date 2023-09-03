using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NSubstitute.AutoSub.Diagnostics;

/// <summary>
/// Handler which tracks the creation of substitutes and mocks so that problems can potentially be diagnosed
/// </summary>
public interface IAutoSubstituteTypeDiagnosticsHandler
{
    /// <summary>
    /// Diagnostic messages broken down by Type with messages about the creation of substitutes/mocks for that type
    /// </summary>
    IReadOnlyDictionary<Type, ReadOnlyCollection<string>> DiagnosticMessages { get; }
}