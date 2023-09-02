using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        var existingListType = typeof(Collection<>).MakeGenericType(type);
        return (IList)Activator.CreateInstance(existingListType);
    }
    

    internal static Type? GetUnderlyingCollectionType(this Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }
            
        return type.GetGenericArguments().Single();
    }
}