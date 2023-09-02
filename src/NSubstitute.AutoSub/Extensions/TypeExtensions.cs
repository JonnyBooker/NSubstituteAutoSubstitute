using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NSubstitute.AutoSub.Extensions;

internal static class TypeExtensions
{
    internal static bool IsCollection(this Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }

        var containsInterfaceCollection = type.GetInterfaces().Any(i => i == typeof(IEnumerable));
        return containsInterfaceCollection;
    }
    
    internal static IList CreateListForType(this Type type)
    {
        var existingListType = typeof(List<>).MakeGenericType(type);
        return (IList)Activator.CreateInstance(type);
    }
}