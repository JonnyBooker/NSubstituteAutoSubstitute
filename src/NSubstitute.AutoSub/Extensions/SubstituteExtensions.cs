using System;
using System.Linq;
using System.Reflection;
using NSubstitute.AutoSub.Exceptions;

namespace NSubstitute.AutoSub.Extensions;

internal static class SubstituteExtensions
{
    internal static object CreateSubstitute(this Type type)
    {
        return type.IsInterface ? 
            Substitute.For(new[] { type }, Array.Empty<object>()) : 
            typeof(Substitute)
                .GetMethod(nameof(Substitute.ForPartsOf))!
                .MakeGenericMethod(type)
                .Invoke(type, new object[] { Array.Empty<object>() });
    }
    
    internal static object CreateExceptionThrowingSubstitute(this Type type)
    {
        var exceptionThrowingMock = Substitute.For(new[] { type }, Array.Empty<object>());

        //All Methods
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => !m.IsSpecialName))
        {
            var exceptionMessage = $"Mock has not been configured for '{type.Name}' when method '{method.Name}' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";
            var parameters = method
                .GetParameters()
                .Select(p => typeof(Arg)
                    .GetMethod(nameof(Arg.Any))!
                    .MakeGenericMethod(p.ParameterType)
                    .Invoke(exceptionThrowingMock, Array.Empty<object>()))
                .ToArray();

            exceptionThrowingMock
                .When(x => { method.Invoke(x, parameters); })
                .Do(_ => throw new AutoSubstituteException(exceptionMessage));
        }

        //All Props
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var exceptionMessage = $"Mock has not been configured for '{type.Name}' when property '{property.Name}' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.";

            if (property.GetMethod is not null)
            {
                exceptionThrowingMock
                    .When(x => { property.GetMethod.Invoke(x, Array.Empty<object>()); })
                    .Do(_ => throw new AutoSubstituteException(exceptionMessage));
            }

            if (property.SetMethod is not null)
            {
                exceptionThrowingMock
                    .When(x => { property.SetMethod.Invoke(x, Array.Empty<object>()); })
                    .Do(_ => throw new AutoSubstituteException(exceptionMessage));
            }
        }

        return exceptionThrowingMock;
    }
}