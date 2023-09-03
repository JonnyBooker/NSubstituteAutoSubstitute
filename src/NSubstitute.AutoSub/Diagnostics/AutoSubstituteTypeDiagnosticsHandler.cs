using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NSubstitute.AutoSub.Diagnostics;

internal class AutoSubstituteTypeDiagnosticsHandler : IAutoSubstituteTypeDiagnosticsHandler
{
    private readonly IDictionary<Type, IList<string>> _diagnosticMessages = new ConcurrentDictionary<Type, IList<string>>();
    
    public IReadOnlyDictionary<Type, ReadOnlyCollection<string>> DiagnosticMessages
    {
        get
        {
            var dictionary = _diagnosticMessages.ToDictionary(x => x.Key, x => new ReadOnlyCollection<string>(x.Value));
            return new ReadOnlyDictionary<Type, ReadOnlyCollection<string>>(dictionary);
        }
    }

    public void AddDiagnosticMessagesForType(Type type, params string[] messages)
    {
        if (!_diagnosticMessages.TryGetValue(type, out var diagnosticGroup))
        {
            diagnosticGroup = new List<string>();
            _diagnosticMessages.Add(type, diagnosticGroup);
        }

        foreach (var message in messages)
        {
            diagnosticGroup.Add(message);
        }
    }
}