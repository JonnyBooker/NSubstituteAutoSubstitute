using System;
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
    
    public AutoSubstitute DidNotReceive<T>(Action<T> expression)
        where T : class
    {
        return ReceivedTimes(expression, 0);
    }
    
    public AutoSubstitute ReceivedOnce<T>(Action<T> expression)
        where T : class
    {
        return ReceivedTimes(expression, 1);
    }
    
    public AutoSubstitute ReceivedTimes<T>(Action<T> expression, int times)
        where T : class
    {
        if (TryGetService(typeof(T), out var mockedInstance) && mockedInstance is not null)
        {
            var castMockedInstance = (T)mockedInstance;
            var received = castMockedInstance.Received(times);
            expression.Invoke(received);

            return this;
        }

        throw new Exception("");
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

    public object CreateInstance(Type instanceType)
    {
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
                    .Where(x => !x.IsCollection())
                    .All(type => currentMockTypes.Contains(type));

                var collectionParameters = constructorParameters
                    .Where(x => x.IsCollection())
                    .ToArray();
                var allCollectionParametersContained = collectionParameters
                    .Select(x => x.GetUnderlyingCollectionType())
                    .Where(x => x is not null)
                    .All(t => currentMockTypes.Contains(t!));

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
                    constructorArguments = mockedConstructorArguments!;
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

    private bool IsValidConstructor(ConstructorInfo potentialConstructor, out object?[]? mockedConstructorArguments)
    {
        var constructorParameters = potentialConstructor
            .GetParameters()
            .Select(x => x.ParameterType)
            .ToArray();

        try
        {
            object?[] constructorArguments = new object[constructorParameters.Length];
            for (var constructorIndex = 0; constructorIndex < constructorParameters.Length; constructorIndex++)
            {
                var constructorParameterType = constructorParameters[constructorIndex];

                //Try and find according to the type given from the constructor first
                var mockExists = TryGetService(constructorParameterType, out var mappedMock);
                
                //If no mock was found and the type is a collection, check the underlying type of the collection
                if (!mockExists && constructorParameterType.IsCollection())
                {
                    var underlyingCollectionType = constructorParameterType.GetUnderlyingCollectionType() ?? throw new Exception("Mer");
                    mockExists = TryGetService(underlyingCollectionType, out mappedMock);

                    //If a single mock is found, wrap it up in a collection and make it the mapped mock
                    if (mockExists)
                    {
                        var emptyCollectionArgument = underlyingCollectionType.CreateListForType();
                        emptyCollectionArgument.Add(mappedMock);
                        mappedMock = emptyCollectionArgument;
                    }
                    else
                    {
                        //If the mock doesn't exist and we are behaving in a non strict way, create an empty collection
                        switch (_behaviour)
                        {
                            case SubstituteBehaviour.LooseFull:
                            case SubstituteBehaviour.LooseParts:
                                var emptyCollectionArgument = underlyingCollectionType.CreateListForType();
                                mappedMock = emptyCollectionArgument;
                                mockExists = true;
                                break;
                        }
                    }
                }

                //If haven't found a mock and the behaviour is loose, create a mock to use
                if (!mockExists && _behaviour != SubstituteBehaviour.Strict)
                {
                    mappedMock = CreateSubstitute(constructorParameterType, () =>
                    {
                        switch (_behaviour)
                        {
                            case SubstituteBehaviour.LooseFull:
                                return Substitute.For(new[] { constructorParameterType }, Array.Empty<object>());
                            case SubstituteBehaviour.LooseParts:
                                return constructorParameterType.IsInterface
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

                //Will be null passed through if it is strict behaviour
                constructorArguments[constructorIndex] = mappedMock;
            }

            //Made it out without issue, this constructor is suitable
            mockedConstructorArguments = constructorArguments;
            return true;
        }
        catch (Exception e)
        {
            //Something went wrong, not going to use this constructor
            mockedConstructorArguments = null;
            return false;
        }
    }

    private object CreateSubstitute(Type mockType, Func<object> actionCreateSubstitute, bool noCache = false)
    {
        //If noCache flag is set, the type map is ignored and a entirely new instance is made and not stored
        if (noCache)
        {
            return actionCreateSubstitute();
        }

        //Check haven't created it before
        if (TryGetService(mockType, out var mappedMockType) && mappedMockType is not null)
        {
            return mappedMockType;
        }

        //Substitute needs creating
        var mockInstance = actionCreateSubstitute();
        _ = _typeMap.TryAdd(mockType, mockInstance);

        return mockInstance;
    }

    private bool TryGetService(Type serviceType, out object? mappedMockType)
    {
        //Try get back according to the specific type give 
        if (!_typeMap.TryGetValue(serviceType, out mappedMockType))
        {
            //If not found, check it isn't a enumerable collection created by this framework
            if (serviceType.IsCollection())
            {
                var underlyingCollectionType = serviceType.GetUnderlyingCollectionType();
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(underlyingCollectionType);

                return _typeMap.TryGetValue(enumerableType, out mappedMockType);
            }

            //Nothing found
            return false;
        }

        //Found a type in the map
        return true;
    }

    object? IServiceProvider.GetService(Type serviceType) => TryGetService(serviceType, out var mappedMockType) ? mappedMockType : null;
}