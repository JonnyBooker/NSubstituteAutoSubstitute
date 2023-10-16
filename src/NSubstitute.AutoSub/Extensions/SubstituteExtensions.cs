using System;
using System.Linq;
using System.Reflection;
using NSubstitute.AutoSub.Exceptions;

namespace NSubstitute.AutoSub.Extensions;

internal static class SubstituteExtensions
{
    internal static object CreateExceptionThrowingSubstitute(this Type type)
    {
        var exceptionThrowingSubstitute = Substitute.For(new[] { type }, Array.Empty<object>());

        //All Methods
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => !m.IsSpecialName))
        {
            var exceptionMessage = $"Substitute has not been configured for '{type.Name}' when method '{method.Name}' was invoked. When using a 'Manual' behaviour, the substitute must be created before 'CreateInstance' is called.";
            var parameters = method
                .GetParameters()
                .Select(p => typeof(Arg)
                    .GetMethod(nameof(Arg.Any))!
                    .MakeGenericMethod(p.ParameterType)
                    .Invoke(exceptionThrowingSubstitute, Array.Empty<object>()))
                .ToArray();

            exceptionThrowingSubstitute
                .When(x => { method.Invoke(x, parameters); })
                .Do(_ => throw new AutoSubstituteException(exceptionMessage));
        }

        //All Props
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var exceptionMessage = $"Substitute has not been configured for '{type.Name}' when property '{property.Name}' was invoked. When using a 'Manual' behaviour, the substitute must be created before 'CreateInstance' is called.";

            if (property.GetMethod is not null)
            {
                exceptionThrowingSubstitute
                    .When(x => { property.GetMethod.Invoke(x, Array.Empty<object>()); })
                    .Do(_ => throw new AutoSubstituteException(exceptionMessage));
            }

            if (property.SetMethod is not null)
            {
                exceptionThrowingSubstitute
                    .When(x => { property.SetMethod.Invoke(x, Array.Empty<object>()); })
                    .Do(_ => throw new AutoSubstituteException(exceptionMessage));
            }
        }

        return exceptionThrowingSubstitute;
    }
}