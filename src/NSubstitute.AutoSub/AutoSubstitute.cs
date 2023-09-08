using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute.AutoSub.Diagnostics;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Extensions;
using NSubstitute.ReceivedExtensions;

namespace NSubstitute.AutoSub;

/// <summary>
/// An auto-mocking IoC container that generates mock objects using NSubstitute.
/// </summary>
public class AutoSubstitute : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, object> _typeMap;
    private readonly SubstituteBehaviour _behaviour;
    private readonly bool _searchPrivateConstructors;
    private readonly AutoSubstituteTypeDiagnosticsHandler _diagnosticsHandler = new();
    
    /// <summary>
    /// Tracks are stores usage of types and substitutes during the creation of instances
    /// </summary>
    public IAutoSubstituteTypeDiagnosticsHandler DiagnosticsHandler => _diagnosticsHandler;

    /// <summary>
    /// Creates a container which can create classes which can be tested with automatically
    /// mocked dependencies
    /// </summary>
    /// <param name="behaviour">Determines how substitutes are generated. <see cref="SubstituteBehaviour"/></param>
    /// <param name="usePrivateConstructors">Check for private constructors to use when creating any instance to test</param>
    public AutoSubstitute(SubstituteBehaviour behaviour = SubstituteBehaviour.LooseParts,
        bool usePrivateConstructors = false)
    {
        _typeMap = new ConcurrentDictionary<Type, object>();
        _behaviour = behaviour;
        _searchPrivateConstructors = usePrivateConstructors;
    }
    
    /// <summary>
    /// Helper to be able more easily verify a action did not take place
    /// </summary>
    /// <param name="expression">Method to verify is not called. Parameters are checked</param>
    /// <typeparam name="T">The dependency to check a method has not been invoked on</typeparam>
    public AutoSubstitute DidNotReceive<T>(Action<T> expression)
        where T : class
    {
        return ReceivedTimes(expression, 0);
    }
    
    /// <summary>
    /// Helper to be able more easily verify a action did take place once
    /// </summary>
    /// <param name="expression">Method to verify is called. Parameters are checked</param>
    /// <typeparam name="T">The dependency to check a method has been invoked on</typeparam>
    public AutoSubstitute ReceivedOnce<T>(Action<T> expression)
        where T : class
    {
        return ReceivedTimes(expression, 1);
    }
    
    /// <summary>
    /// Helper to be able more easily verify a action did take place once
    /// </summary>
    /// <param name="expression">Method to verify is called. Parameters are checked</param>
    /// <typeparam name="T">The dependency to check a method has been invoked on</typeparam>
    public AutoSubstitute ReceivedAtLeastOnce<T>(Action<T> expression)
        where T : class
    {
        return ReceivedTimes(expression, Quantity.AtLeastOne());
    }

    /// <summary>
    /// Helper to be able more easily verify a action did take place a defined number of times
    /// </summary>
    /// <param name="expression">Method to verify is called. Parameters are checked</param>
    /// <param name="times">The amount of times a method is expected to be called</param>
    /// <typeparam name="T">The dependency to check a method has been invoked on</typeparam>
    public AutoSubstitute ReceivedTimes<T>(Action<T> expression, int times)
        where T : class
    {
        return ReceivedTimes(expression, Quantity.Exactly(times));
    }
    /// <summary>
    /// Helper to be able more easily verify a action did take place a defined number of times
    /// </summary>
    /// <param name="expression">Method to verify is called. Parameters are checked</param>
    /// <param name="times">The amount of times a method is expected to be called</param>
    /// <typeparam name="T">The dependency to check a method has been invoked on</typeparam>
    public AutoSubstitute ReceivedTimes<T>(Action<T> expression, Quantity times)
        where T : class
    {
        if (TryGetService(typeof(T), out var mockedInstance) && mockedInstance is not null)
        {
            var castMockedInstance = (T)mockedInstance;
            var received = castMockedInstance.Received(times);
            expression.Invoke(received);

            return this;
        }

        switch (_behaviour)
        {
            case SubstituteBehaviour.LooseFull:
            case SubstituteBehaviour.LooseParts:
                throw new Exception("Could not find mocked service. This should not have happened but a workaround would be to utilise the 'Use'/'UseCollection' methods to ensure there is a implementation used.");
            case SubstituteBehaviour.Strict:
                throw new Exception($"Could not find mocked service. Substitute behaviour is 'Strict' so unless you have explicitly utilised the '{typeof(T).Name}' type or utilise 'Use'/'UseCollection', the dependency will be null and cannot be checked via this method.");
            default:
                throw new ArgumentOutOfRangeException(null, "Unable to verify received at this time.");
        }
    }

    /// <summary>
    /// Searches or creates a substitute that a system under test can use that is not
    /// cached and is newly created everytime.
    /// Underneath this will use <see cref="Substitute.For{T}"/>.
    /// </summary>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T SubstituteForNoCache<T>() where T : class => SubstituteFor<T>(true);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use that is not
    /// cached and is newly created everytime.
    /// Underneath this will use <see cref="Substitute.ForPartsOf{T}"/>.
    /// </summary>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T SubstituteForPartsOfNoCache<T>() where T : class => SubstituteForPartsOf<T>(true);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use. Underneath this will use <see cref="Substitute.For{T}"/>.
    /// </summary>
    /// <param name="noCache">Option if want to create an instance that is newly created and not stored</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T SubstituteFor<T>(bool noCache = false) where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.For<T>(), noCache);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use. Underneath this will use <see cref="Substitute.ForPartsOf{T}"/>.
    /// </summary>
    /// <param name="noCache">Option if want to create an instance that is newly created and not stored</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T SubstituteForPartsOf<T>(bool noCache = false) where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.ForPartsOf<T>(), noCache);

    /// <summary>
    /// Forces a specific type instance to be used over creating a substituted instance automatically
    /// </summary>
    /// <param name="instance">The instance to use during the creation of a system under test</param>
    /// <typeparam name="T">The type of the instance</typeparam>
    public void Use<T>(T instance) where T : class
    {
        var instanceType = typeof(T);
        _ = _typeMap.TryRemove(instanceType, out _);
        _ = _typeMap.TryAdd(instanceType, instance);
    }

    /// <summary>
    /// Used to be able to provide multiple mock instances for enumerable parameters. This can be
    /// hard instances or multiple substitute instances that want to be used as part of the system
    /// under test
    /// </summary>
    /// <param name="instances">Instances to be injected into a system under test</param>
    /// <typeparam name="T">The base/interface type of the instances being passed in</typeparam>
    public void UseCollection<T>(params T[] instances) where T : class
    {
        var collectionType = typeof(IEnumerable<T>);
        _ = _typeMap.TryRemove(collectionType, out _);
        _ = _typeMap.TryAdd(collectionType, new List<T>(instances).AsEnumerable());
    }

    /// <summary>
    /// Create a class instance to test using the substituted dependencies. Any dependencies not
    /// mocked will follow the <see cref="SubstituteBehaviour"/> set when this class was created
    /// </summary>
    /// <typeparam name="T">The type to create</typeparam>
    /// <returns>A instance to test according to the type passed in</returns>
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed</exception>
    public T CreateInstance<T>() where T : class
    {
        return (T)CreateInstance(typeof(T));
    }
    
    /// <summary>
    /// Create a class instance to test using the substituted dependencies. Any dependencies not
    /// mocked will follow the <see cref="SubstituteBehaviour"/> set when this class was created.
    ///
    /// This will be passed back as a object type.
    /// </summary>
    /// <param name="instanceType">The type to create</param>
    /// <returns>A instance to test according to the instance type passed in, passed back as a generic object</returns>
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed</exception>
    public object CreateInstance(Type instanceType)
    {
        var bindingFlags = !_searchPrivateConstructors ? 
            BindingFlags.Instance | BindingFlags.Public : 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var potentialConstructors = instanceType
            .GetConstructors(bindingFlags)
            .OrderByDescending(x => x.GetParameters().Length)
            .ToList();
        
        _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Found {potentialConstructors.Count} potential constructors");

        ConstructorInfo? bestConstructor = null;
        object[]? constructorArguments = null;

        var currentMockTypes = _typeMap.Keys;
        foreach (var potentialConstructor in potentialConstructors)
        {
            var constructorParameters = potentialConstructor!.GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();
            
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Checking constructor using '{_behaviour.ToString()} behaviour. Parameters: {string.Join(", ", constructorParameters.Select(x => x.Name))}");

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
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Found best constructor!");
                    
                    bestConstructor = potentialConstructor;
                    constructorArguments = new object[constructorParameters.Length];
                    break;
                }
            }
            else
            {
                if (IsValidConstructor(instanceType, potentialConstructor, out var mockedConstructorArguments))
                {
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Found best constructor!");
                    
                    bestConstructor = potentialConstructor;
                    constructorArguments = mockedConstructorArguments!;
                    break;
                }

                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Unsuitable constructor...");
                
                bestConstructor = null;
                constructorArguments = null;
            }
        }

        if (bestConstructor is null)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "No suitable constructor found for type");
            throw new AutoSubstituteException("Unable to find suitable constructor");
        }

        return bestConstructor.Invoke(constructorArguments ?? Array.Empty<object>());
    }

    private bool IsValidConstructor(Type instanceTypeForConstructor, ConstructorInfo potentialConstructor, out object?[]? mockedConstructorArguments)
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
                
                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Checking Mock for {constructorParameterType} type");

                //Try and find according to the type given from the constructor first
                var mockExists = TryGetService(constructorParameterType, out var mappedMock);
                
                //If no mock was found and the type is a collection, check the underlying type of the collection
                if (!mockExists && constructorParameterType.IsCollection())
                {
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Mock was collection type, seeing if can find a mocked collection version of type");
                    
                    var underlyingCollectionType = constructorParameterType.GetUnderlyingCollectionType() ?? throw new AutoSubstituteException("Unable to get underlying collection type");
                    mockExists = TryGetService(underlyingCollectionType, out mappedMock);

                    //If a single mock is found, wrap it up in a collection and make it the mapped mock
                    if (mockExists)
                    {
                        _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Found single instance for collection mock type. Will use this!");
                        
                        var mockedCollectionList = underlyingCollectionType.CreateListForType();
                        mockedCollectionList.Add(mappedMock);
                        mappedMock = mockedCollectionList;
                    }
                    else
                    {
                        //If the mock doesn't exist and we are behaving in a non strict way, create an empty collection
                        switch (_behaviour)
                        {
                            case SubstituteBehaviour.LooseFull:
                            case SubstituteBehaviour.LooseParts:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Behaviour was loose so will create an empty collection of dependency type");
                                
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
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Creating a substitute for type: {constructorParameterType}");
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
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Error happened checking eligibility of this constructor. Error: {e.Message}");
            
            mockedConstructorArguments = null;
            return false;
        }
    }

    private object CreateSubstitute(Type mockType, Func<object> actionCreateSubstitute, bool noCache = false)
    {
        //If noCache flag is set, the type map is ignored and a entirely new instance is made and not stored
        if (noCache)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, $"Creating a non cached substitute");
            return actionCreateSubstitute();
        }

        //Check haven't created it before
        if (TryGetService(mockType, out var mappedMockType) && mappedMockType is not null)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, $"Existing substitute found. Will use this!");
            return mappedMockType;
        }

        //Substitute needs creating
        _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, $"Substitute not found. Will create...");
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