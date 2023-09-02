using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute.AutoSub.Extensions;

namespace NSubstitute.AutoSub;

public class AutoSubstitute : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, object> _typeMap;
    private readonly SubstituteBehaviour _behaviour;
    private readonly bool _searchPrivateConstructors;

    public AutoSubstitute(SubstituteBehaviour behaviour = SubstituteBehaviour.LooseParts,
        bool usePrivateConstructors = false)
    {
        _typeMap = new ConcurrentDictionary<Type, object>();
        _behaviour = behaviour;
        _searchPrivateConstructors = usePrivateConstructors;
    }
    
    public T SubstituteForNoCache<T>() where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.For<T>(), true);
    
    public T SubstituteForPartsOfNoCache<T>() where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.ForPartsOf<T>(), true);

    public T SubstituteFor<T>(bool noCache = false) where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.For<T>(), noCache);

    public T SubstituteForPartsOf<T>(bool noCache = false) where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.ForPartsOf<T>(), noCache);

    public void UseSubstitute<T>(T instance) where T : class
    {
        var instanceType = typeof(T);
        _ = _typeMap.TryRemove(instanceType, out _);
        _ = _typeMap.TryAdd(instanceType, instance);
    }

    public void UseSubstituteCollection<T>(params T[] instances) where T : class
    {
        var collectionType = typeof(IEnumerable<T>);
        _ = _typeMap.TryRemove(collectionType, out _);
        _ = _typeMap.TryAdd(collectionType, new List<T>(instances).AsEnumerable());
    }

    public T CreateInstance<T>()
    {
        return (T)CreateInstance(typeof(T));
    }

    public object CreateInstance(Type type)
    {
        var instanceType = type;
        var bindingFlags = !_searchPrivateConstructors ? BindingFlags.Instance | BindingFlags.Public : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var potentialConstructors = instanceType
            .GetConstructors(bindingFlags)
            .OrderByDescending(x => x.GetParameters().Length);

        ConstructorInfo? bestConstructor = null;
        object[]? constructorArguments = null;

        var currentMockTypes = _typeMap.Keys;
        foreach (var potentialConstructor in potentialConstructors)
        {
            var constructorParameters = potentialConstructor!.GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();

            if (_behaviour == SubstituteBehaviour.Strict)
            {
                var allParametersContained = constructorParameters
                    .Where(x => !x.IsArray)
                    .All(type => currentMockTypes.Contains(type));

                var collectionParameters = constructorParameters
                    .Where(x => x.IsGenericType)
                    .ToArray();
                var allCollectionParametersContained = collectionParameters
                    .Select(x => x.GetElementType())
                    .All(type => currentMockTypes.Contains(type));

                if (allParametersContained && (collectionParameters.Any() && allCollectionParametersContained))
                {
                    bestConstructor = potentialConstructor;
                    constructorArguments = new object[constructorParameters.Length];
                    break;
                }
            }
            else
            {
                if (IsValidConstructor(potentialConstructor, out var mockedConstructorArguments))
                {
                    bestConstructor = potentialConstructor;
                    constructorArguments = mockedConstructorArguments;
                    break;
                }

                bestConstructor = null;
                constructorArguments = null;
            }
        }

        if (bestConstructor is null)
        {
            throw new Exception("Unable to find suitable constructor");
        }

        return bestConstructor.Invoke(constructorArguments ?? Array.Empty<object>());
    }

    private bool IsValidConstructor(ConstructorInfo potentialConstructor, out object[]? mockedConstructorArguments)
    {
        var constructorParameters = potentialConstructor
            .GetParameters()
            .Select(x => x.ParameterType)
            .ToArray();

        try
        {
            var constructorArguments = new object[constructorParameters.Length];
            for (var constructorIndex = 0; constructorIndex < constructorParameters.Length; constructorIndex++)
            {
                var constructorParameterType = constructorParameters[constructorIndex];
                var constructorParameterIsCollection = constructorParameterType.IsCollection();

                var mockExists = TryGetService(constructorParameterType, out var mappedMock);
                if (!mockExists)
                {
                    if (constructorParameterIsCollection)
                    {
                        var underlyingType = constructorParameterType.GetUnderlyingCollectionType();
                        constructorParameterType = underlyingType ?? throw new Exception("Unable to create mock for collection type");
                    }
                    
                    mappedMock = CreateSubstitute(constructorParameterType, () =>
                    {
                        switch (_behaviour)
                        {
                            case SubstituteBehaviour.LooseFull:
                                return Substitute.For(new[] { constructorParameterType }, Array.Empty<object>());
                            case SubstituteBehaviour.LooseParts:
                                return constructorParameterType!.IsInterface
                                    ? Substitute.For(new[] { constructorParameterType }, Array.Empty<object>())
                                    : typeof(Substitute)
                                        .GetMethod(nameof(Substitute.ForPartsOf))!
                                        .MakeGenericMethod(constructorParameterType)
                                        .Invoke(this, new object[] { Array.Empty<object>() });
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                }
                
                if (!mockExists && constructorParameterIsCollection)
                {
                    var collectionArgument = constructorParameterType.CreateListForType();
                    collectionArgument.Add(mappedMock);
                    constructorArguments[constructorIndex] = collectionArgument;
                }
                else
                {
                    constructorArguments[constructorIndex] = mappedMock;
                }
            }

            mockedConstructorArguments = constructorArguments;

            return true;
        }
        catch (Exception e)
        {
            mockedConstructorArguments = null;
            return false;
        }
    }

    private object CreateSubstitute(Type mockType, Func<object> actionCreateSubstitute, bool noCache = false)
    {
        if (noCache)
        {
            return actionCreateSubstitute();
        }

        if (_typeMap.TryGetValue(mockType, out var mappedMockType))
        {
            return mappedMockType;
        }

        var mockInstance = actionCreateSubstitute();
        _ = _typeMap.TryAdd(mockType, mockInstance);

        return mockInstance;
    }

    private bool TryGetService(Type serviceType, out object mappedMockType) => _typeMap.TryGetValue(serviceType, out mappedMockType);

    object? IServiceProvider.GetService(Type serviceType) => TryGetService(serviceType, out var mappedMockType) ? mappedMockType : null;
}