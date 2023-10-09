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
    public AutoSubstitute(SubstituteBehaviour behaviour = SubstituteBehaviour.Automatic,
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
            case SubstituteBehaviour.Automatic:
                throw new AutoSubstituteException("Could not find mocked service. This typically should not have happened but a workaround would be to utilise the 'Use'/'UseCollection' methods to ensure there is a implementation used.");
            case SubstituteBehaviour.ManualWithNulls:
            case SubstituteBehaviour.ManualWithExceptions:
                throw new AutoSubstituteException($"Could not find mocked service. Substitute behaviour is a 'Manual' behaviour, so unless you have explicitly utilised the '{typeof(T).Name}' type or utilise 'Use'/'UseCollection', the dependency cannot be checked via this method.");
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
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed or anything goes wrong</exception>
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
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed or anything goes wrong</exception>
    public object CreateInstance(Type instanceType)
    {
        var bindingFlags = !_searchPrivateConstructors ? 
            BindingFlags.Instance | BindingFlags.Public : 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var potentialConstructors = instanceType
            .GetConstructors(bindingFlags)
            .OrderByDescending(x => x.GetParameters().Length)
            .ToList();
        
        _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Found '{potentialConstructors.Count}' potential constructors");

        ConstructorInfo? bestConstructor = null;
        var constructorArguments = Array.Empty<object?>();
        
        foreach (var potentialConstructor in potentialConstructors)
        {
            var constructorParameterTypes = potentialConstructor!.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Checking constructor using '{_behaviour.ToString()}' behaviour. Parameters: {string.Join(", ", constructorParameterTypes.Select(x => x.Name))}");

            if (_behaviour != SubstituteBehaviour.Automatic)
            {
                var allParametersContained = constructorParameterTypes
                    .Where(t => !t.IsCollection())
                    .All(type => _typeMap.Keys.Contains(type));

                var collectionParameters = constructorParameterTypes
                    .Where(t => t.IsCollection())
                    .ToArray();
                var allCollectionParametersContained = collectionParameters
                    .Select(t => t.GetUnderlyingCollectionType())
                    .Where(t => t is not null)
                    .All(t => _typeMap.Keys.Contains(t!));

                if (allParametersContained && (!collectionParameters.Any() || allCollectionParametersContained))
                {
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Found best constructor!");
                    
                    bestConstructor = potentialConstructor;
                    constructorArguments = new object[constructorParameterTypes.Length];
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

                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Unsuitable constructor...");
            }
        }
        
        //If are operating on a non automatic mode mode, then fallback to the "greediest"/largest constructor
        if (_behaviour != SubstituteBehaviour.Automatic)
        {
            bestConstructor = potentialConstructors.FirstOrDefault();
            if (bestConstructor is not null && IsValidConstructor(instanceType, bestConstructor, out var mockedConstructorArguments))
            {
                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Falling back to largest constructor as using 'Manual' behaviour. Parameters: {bestConstructor.GetParameters().Select(t => t.Name)}");
                constructorArguments = mockedConstructorArguments;
            }
            else
            {
                bestConstructor = null;
            }
        }

        //We tried... Can't do no more...
        if (bestConstructor is null)
        {
            string errorMessage;
            switch (_behaviour)
            {
                case SubstituteBehaviour.ManualWithNulls:
                    errorMessage = "Unable to find suitable constructor. " +
                                   "You are using 'Manual with Nulls' behaviour mode, a mock must be created for the method before the system under test instance is created. " +
                                   "Alternatively, use an 'Automatic' behaviour mode or you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";
                    break;
                case SubstituteBehaviour.ManualWithExceptions:
                    errorMessage = "Unable to find suitable constructor. " +
                                   "You are using 'Manual with Exceptions' behaviour mode, the type you are create as a system under test may contain concrete implementations that are not suitable for this behaviour mode. " +
                                   "'Use' can be used to ensure that these classes get the implementations that are expected to work with this behaviour mode. " +
                                   "Alternatively, use an 'Automatic' behaviour mode or you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";
                    break;
                default:
                    errorMessage = "Unable to find suitable constructor. " +
                                   "Ensure there is a constructor that is accessible (i.e. public) and its constructor parameter types are also accessible. " +
                                   "Alternatively, you can use 'usePrivateConstructors' when 'AutoSubstitute' is created.";
                    break;
            }
            
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "No suitable constructor found for type");
            throw new AutoSubstituteException(errorMessage);
        }

        return bestConstructor.Invoke(constructorArguments);
    }

    private bool IsValidConstructor(Type instanceTypeForConstructor, ConstructorInfo potentialConstructor, out object?[] mockedConstructorArguments)
    {
        var constructorParameters = potentialConstructor
            .GetParameters()
            .Select(x => x.ParameterType)
            .ToArray();

        try
        {
            var constructorArguments = new object?[constructorParameters.Length];
            for (var constructorIndex = 0; constructorIndex < constructorParameters.Length; constructorIndex++)
            {
                var constructorParameterType = constructorParameters[constructorIndex];
                
                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Checking Mock for '{constructorParameterType.Name}' type");

                //Try and find according to the type given from the constructor first
                if (!TryGetService(constructorParameterType, out var mappedMock))
                {
                    //The type is a collection, check the underlying type of the collection
                    if (constructorParameterType.IsCollection())
                    {
                        _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Mock was collection type, seeing if can find a mocked collection version of type");
                    
                        var underlyingCollectionType = constructorParameterType.GetUnderlyingCollectionType() ?? throw new AutoSubstituteException($"Unable to get underlying collection type for: {constructorParameterType.Name}");

                        //If a single mock is found, wrap it up in a collection and make it the mapped mock
                        if (TryGetService(underlyingCollectionType, out mappedMock))
                        {
                            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Found single instance for collection mock type. Will use this!");
                        
                            var mockedCollectionList = underlyingCollectionType.CreateListForType();
                            mockedCollectionList.Add(mappedMock);
                            mappedMock = mockedCollectionList;
                        }
                        else
                        {
                            switch (_behaviour)
                            {
                                case SubstituteBehaviour.Automatic:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Automatic' so will create an empty collection of dependency type");
                                    mappedMock = underlyingCollectionType.CreateListForType();
                                    break;
                                case SubstituteBehaviour.ManualWithNulls:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Nulls' so mock will be a null collection");
                                    mappedMock = null;
                                    break;
                                case SubstituteBehaviour.ManualWithExceptions:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Exceptions' so mock will be a collection with an exception throwing substitute");
                                    
                                    var mockedCollectionList = underlyingCollectionType.CreateListForType();
                                    var exceptionThrowingMock = CreateExceptionThrowingMockOrThrow(underlyingCollectionType, constructorParameterType);
                                    
                                    mockedCollectionList.Add(exceptionThrowingMock);
                                    mappedMock = mockedCollectionList;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (_behaviour)
                        {
                            case SubstituteBehaviour.Automatic:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Creating a substitute for type: {constructorParameterType}");
                                mappedMock = CreateSubstitute(constructorParameterType, () => constructorParameterType.CreateSubstitute());
                                break;
                            case SubstituteBehaviour.ManualWithNulls:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Nulls' so mock will be a null");
                                mappedMock = null;
                                break;
                            case SubstituteBehaviour.ManualWithExceptions:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Exceptions' so mock will be an exception throwing substitute");
                                mappedMock = CreateExceptionThrowingMockOrThrow(constructorParameterType, constructorParameterType);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(_behaviour), "Behaviour is not supported. This is the fault of AutoSubstitute and should be fixed at a framework level.");
                        }
                    }
                }
                
                //Put whatever form came out into the constructor arguments
                constructorArguments[constructorIndex] = mappedMock;
            }

            //Constructor is suitable
            mockedConstructorArguments = constructorArguments;
            return true;
        }
        catch (Exception ex)
        {
            //Something went wrong, not going to use this constructor
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Error happened checking eligibility of this constructor. Error: {ex.Message}");
            mockedConstructorArguments = Array.Empty<object>();
            return false;
        }
    }

    private object CreateSubstitute(Type mockType, Func<object> actionCreateSubstitute, bool noCache = false)
    {
        //If flag is set, the type map is ignored and a entirely new instance is made and not stored to be use
        if (noCache)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, "Creating a non cached substitute");
            return actionCreateSubstitute();
        }

        //Check haven't created it before
        if (TryGetService(mockType, out var mappedMockType) && mappedMockType is not null)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, "Existing substitute found. Will use this!");
            return mappedMockType;
        }

        //Substitute needs creating
        _diagnosticsHandler.AddDiagnosticMessagesForType(mockType, "Substitute not found. Will create and store this for later use");
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

    private object CreateExceptionThrowingMockOrThrow(Type constructorParameterType, Type diagnosticParameterType)
    {
        if (constructorParameterType.IsInterface)
        {
            return constructorParameterType.CreateExceptionThrowingSubstitute();
        }

        _diagnosticsHandler.AddDiagnosticMessagesForType(diagnosticParameterType, "When using behaviour 'Manual with Exceptions', only interfaces are usable with this behaviour"); 
        throw new AutoSubstituteException("Only interfaces are usable when using 'Manual with Exceptions' behaviour");
    }

    object? IServiceProvider.GetService(Type serviceType) => TryGetService(serviceType, out var mappedMockType) ? mappedMockType : null;
}