using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NSubstitute.AutoSub.Diagnostics;

public interface IAutoSubstituteTypeDiagnosticsHandler
{
    IReadOnlyDictionary<Type, ReadOnlyCollection<string>> DiagnosticMessages { get; }
}