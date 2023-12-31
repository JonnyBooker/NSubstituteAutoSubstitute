﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute.AutoSub.Classes;
using NSubstitute.AutoSub.Classes.Enums;
using NSubstitute.AutoSub.Diagnostics;
using NSubstitute.AutoSub.Exceptions;
using NSubstitute.AutoSub.Extensions;
using NSubstitute.ReceivedExtensions;

namespace NSubstitute.AutoSub;

/// <summary>
/// An auto-substituting IoC container that generates mocks using NSubstitute.
/// </summary>
public class AutoSubstitute : IServiceProvider
{
    private readonly ConcurrentDictionary<Type, SubstituteTypeObject> _substituteTypeMap;
    private readonly SubstituteBehaviour _behaviour;
    private readonly bool _searchPrivateConstructors;
    private readonly AutoSubstituteTypeDiagnosticsHandler _diagnosticsHandler = new();

    /// <summary>
    /// Tracks are stores usage of types and substitutes during the creation of instances
    /// </summary>
    public IAutoSubstituteTypeDiagnosticsHandler DiagnosticsHandler => _diagnosticsHandler;

    /// <summary>
    /// Creates a container which can create classes which can be tested with automatically
    /// substituted dependencies
    /// </summary>
    /// <param name="behaviour">Determines how substitutes are generated. <see cref="SubstituteBehaviour"/></param>
    /// <param name="usePrivateConstructors">Check for private constructors to use when creating any instance to test</param>
    public AutoSubstitute(SubstituteBehaviour behaviour = SubstituteBehaviour.Automatic,
        bool usePrivateConstructors = false)
    {
        _substituteTypeMap = new ConcurrentDictionary<Type, SubstituteTypeObject>();
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
        if (TryGetService(typeof(T), out var substituteTypeObject))
        {
            var castedInstance = (T)substituteTypeObject.Instance;
            var received = castedInstance.Received(times);
            expression.Invoke(received);

            return this;
        }

        switch (_behaviour)
        {
            case SubstituteBehaviour.Automatic:
                throw new AutoSubstituteException("Could not find substituted service. This typically should not have happened but a workaround would be to utilise the 'Use'/'UseCollection' methods to ensure there is a implementation used.");
            case SubstituteBehaviour.ManualWithNulls:
            case SubstituteBehaviour.ManualWithExceptions:
                throw new AutoSubstituteException($"Could not find substituted service. Substitute behaviour is a 'Manual' behaviour, so unless you have explicitly utilised the '{typeof(T).Name}' type or utilise 'Use'/'UseCollection', the dependency cannot be checked via this method.");
            default:
                throw new ArgumentOutOfRangeException(null, "Unable to verify received at this time.");
        }
    }

    /// <summary>
    /// Searches or creates a substitute that a system under test can use that is not cached and is newly created
    /// everytime.
    /// Underneath this will use <see cref="Substitute.For{T}"/>.
    /// </summary>
    /// <param name="constructorArguments">Arguments required to construct a class being substituted. Not required for interfaces or classes with default constructors.</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T GetSubstituteForNoTracking<T>(params object[] constructorArguments) where T : class => 
        (T)CreateSubstitute(typeof(T), () => Substitute.For<T>(constructorArguments), SubstituteType.For, true);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use that is not cached and is newly created
    /// everytime. Note these arguments will only be passed once when the substitute is first created.
    /// Underneath this will use <see cref="Substitute.ForPartsOf{T}"/>.
    /// </summary>
    /// <param name="constructorArguments">Arguments required to construct a class being substituted. Not required for interfaces or classes with default constructors.</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T GetSubstituteForPartsOfNoTracking<T>(params object[] constructorArguments) where T : class =>
        (T)CreateSubstitute(typeof(T), () => Substitute.ForPartsOf<T>(constructorArguments), SubstituteType.For, true);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use. Note any constructor arguments passed
    /// will only be passed once when the substitute is first created.
    /// Underneath this will use <see cref="Substitute.For{T}"/>.
    /// </summary>
    /// <param name="constructorArguments">Arguments required to construct a class being substituted. Not required for interfaces or classes with default constructors.</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T GetSubstituteFor<T>(params object[] constructorArguments) where T : class =>
        (T) CreateSubstitute(typeof(T), () => Substitute.For<T>(constructorArguments), SubstituteType.For);

    /// <summary>
    /// Searches or creates a substitute that a system under test can use. Note any constructor arguments passed
    /// will only be passed once when the substitute is first created.
    /// Underneath this will use <see cref="Substitute.ForPartsOf{T}"/>.
    /// </summary>
    /// <param name="constructorArguments">Arguments required to construct a class being substituted. Not required for interfaces or classes with default constructors.</param>
    /// <typeparam name="T">The class or interface to search</typeparam>
    /// <returns>A substitute for the specified class or interface</returns>
    public T GetSubstituteForPartsOf<T>(params object[] constructorArguments) where T : class =>
        (T) CreateSubstitute(typeof(T), () => Substitute.ForPartsOf<T>(constructorArguments), SubstituteType.ForPartsOf);

    /// <summary>
    /// Forces a specific type instance to be used over creating a substituted instance automatically
    /// </summary>
    /// <param name="instance">The instance to use during the creation of a system under test</param>
    /// <typeparam name="T">The type of the instance</typeparam>
    public void Use<T>(T instance) where T : class
    {
        var instanceType = typeof(T);
        _ = _substituteTypeMap.TryRemove(instanceType, out _);
        _ = _substituteTypeMap.TryAdd(instanceType, new SubstituteTypeObject(SubstituteType.Manual, instance));
    }

    /// <summary>
    /// Used to be able to provide multiple substituted instances for enumerable parameters. This can be
    /// explicitly created or substitute instances that want to be used as part of the system under test
    /// </summary>
    /// <param name="instances">Instances to be injected into a system under test</param>
    /// <typeparam name="T">The base/interface type of the instances being passed in</typeparam>
    public void UseCollection<T>(params T[] instances) where T : class
    {
        var collectionType = typeof(IEnumerable<T>);
        _ = _substituteTypeMap.TryRemove(collectionType, out _);
        _ = _substituteTypeMap.TryAdd(collectionType, new SubstituteTypeObject(SubstituteType.Manual, new List<T>(instances).AsEnumerable()));
    }

    /// <summary>
    /// Create a class instance to test using substituted dependencies. Any dependencies not
    /// substituted will follow the <see cref="SubstituteBehaviour"/> set when this class was created
    /// </summary>
    /// <typeparam name="T">The type to create</typeparam>
    /// <returns>An instance to test according to the type passed in</returns>
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed or anything goes wrong</exception>
    public T CreateInstance<T>() where T : class
    {
        return (T)CreateInstance(typeof(T));
    }

    /// <summary>
    /// Create a class instance to test using the substituted dependencies. Any dependencies not
    /// substituted will follow the <see cref="SubstituteBehaviour"/> set when this class was created.
    ///
    /// This will be passed back as a object type.
    /// </summary>
    /// <param name="instanceType">The type to create</param>
    /// <returns>An instance to test according to the instance type passed in, passed back as a generic object</returns>
    /// <exception cref="AutoSubstituteException">Thrown if the instance cannot be constructed or anything goes wrong</exception>
    public object CreateInstance(Type instanceType)
    {
        var bindingFlags = !_searchPrivateConstructors ? BindingFlags.Instance | BindingFlags.Public : BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
                    .All(type => _substituteTypeMap.Keys.Contains(type));

                var collectionParameters = constructorParameterTypes
                    .Where(t => t.IsCollection())
                    .ToArray();
                var allCollectionParametersContained = collectionParameters
                    .Select(t => t.GetUnderlyingCollectionType())
                    .Where(t => t is not null)
                    .All(t => _substituteTypeMap.Keys.Contains(t!));

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
                if (IsValidConstructor(instanceType, potentialConstructor, out var substitutedConstructorArguments))
                {
                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Found best constructor!");

                    bestConstructor = potentialConstructor;
                    constructorArguments = substitutedConstructorArguments;
                    break;
                }

                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, "Unsuitable constructor...");
            }
        }

        //If are operating on a non automatic mode mode, then fallback to the "greediest"/largest constructor
        if (_behaviour != SubstituteBehaviour.Automatic)
        {
            bestConstructor = potentialConstructors.FirstOrDefault();
            if (bestConstructor is not null && IsValidConstructor(instanceType, bestConstructor, out var substitutedConstructorArguments))
            {
                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceType, $"Falling back to largest constructor as using a 'Manual' behaviour. Parameters: {bestConstructor.GetParameters().Select(t => t.Name)}");
                constructorArguments = substitutedConstructorArguments;
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
                                   "You are using 'Manual with Nulls' behaviour mode, a substitute must be created for the method before the system under test instance is created. " +
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

    private bool IsValidConstructor(Type instanceTypeForConstructor, ConstructorInfo potentialConstructor, out object?[] substitutedConstructorArguments)
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
                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Checking for '{constructorParameterType.Name}' substitute type");

                //Try and find according to the type given from the constructor first
                object? mappedSubstitute = null;
                if (!TryGetService(constructorParameterType, out var substituteTypeObject))
                {
                    //The type is a collection, check the underlying type of the collection
                    if (constructorParameterType.IsCollection())
                    {
                        _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Type requested was a collection type, seeing if can find a substituted collection version of type");

                        var underlyingCollectionType = constructorParameterType.GetUnderlyingCollectionType() ?? throw new AutoSubstituteException($"Unable to get underlying collection type for: {constructorParameterType.Name}");

                        //If a single substitute is found, wrap it up in a collection and make it the mapped substitute
                        if (TryGetService(underlyingCollectionType, out substituteTypeObject))
                        {
                            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Found single instance for collection substitute type. Will use this!");

                            var substitutedCollectionList = underlyingCollectionType.CreateListForType();
                            substitutedCollectionList.Add(substituteTypeObject.Instance);
                            mappedSubstitute = substitutedCollectionList;
                        }
                        else
                        {
                            switch (_behaviour)
                            {
                                case SubstituteBehaviour.Automatic:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Automatic' so will create an empty collection of dependency type");
                                    mappedSubstitute = underlyingCollectionType.CreateListForType();
                                    break;
                                case SubstituteBehaviour.ManualWithNulls:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Nulls' so substitute will be a null collection");
                                    mappedSubstitute = null;
                                    break;
                                case SubstituteBehaviour.ManualWithExceptions:
                                    _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Exceptions' so substitute will be a collection with an exception throwing substitute");

                                    var substitutedCollectionList = underlyingCollectionType.CreateListForType();
                                    var exceptionThrowingInstance = CreateExceptionThrowingSubstituteOrThrow(underlyingCollectionType, constructorParameterType);

                                    substitutedCollectionList.Add(exceptionThrowingInstance);
                                    mappedSubstitute = substitutedCollectionList;
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
                                mappedSubstitute = CreateSubstitute(constructorParameterType, () => Substitute.For(new[] { constructorParameterType }, Array.Empty<object>()), SubstituteType.For);
                                break;
                            case SubstituteBehaviour.ManualWithNulls:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Nulls' so substitute will be a null");
                                mappedSubstitute = null;
                                break;
                            case SubstituteBehaviour.ManualWithExceptions:
                                _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, "Behaviour was 'Manual with Exceptions' so substitute will be an exception throwing substitute");
                                mappedSubstitute = CreateExceptionThrowingSubstituteOrThrow(constructorParameterType, constructorParameterType);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(_behaviour), "Behaviour is not supported. This is the fault of AutoSubstitute and should be fixed at a framework level.");
                        }
                    }
                }
                else
                {
                    mappedSubstitute = substituteTypeObject.Instance;
                }

                //Put whatever form came out into the constructor arguments
                constructorArguments[constructorIndex] = mappedSubstitute;
            }

            //Constructor is suitable
            substitutedConstructorArguments = constructorArguments;
            return true;
        }
        catch (Exception ex)
        {
            //Something went wrong, not going to use this constructor
            _diagnosticsHandler.AddDiagnosticMessagesForType(instanceTypeForConstructor, $"Error happened checking eligibility of this constructor. Error: {ex.Message}");
            substitutedConstructorArguments = Array.Empty<object>();
            return false;
        }
    }

    private object CreateSubstitute(Type type, Func<object> actionCreateSubstitute, SubstituteType substituteType, bool noCache = false)
    {
        //If flag is set, the type map is ignored and a entirely new instance is made and not stored to be use
        if (noCache)
        {
            _diagnosticsHandler.AddDiagnosticMessagesForType(type, "Creating a non cached substitute");
            return actionCreateSubstitute();
        }

        //Check haven't created it before
        if (TryGetService(type, out var substituteTypeObject))
        {
            //Substitutes have been created using mixed methods, throw an exception and advise
            if (substituteTypeObject.SubstituteType != SubstituteType.Manual && substituteTypeObject.SubstituteType != substituteType)
            {
                _diagnosticsHandler.AddDiagnosticMessagesForType(type, $"Substitute type has been created using mixed methods. Only one of these should be used, e.g. {nameof(GetSubstituteFor)}/{nameof(GetSubstituteForPartsOf)}/{nameof(Use)}/{nameof(UseCollection)}");
                throw new AutoSubstituteException($"Substitute type '{type.Name}' has been created using mixed methods. Only one of these (e.g. {nameof(GetSubstituteFor)}/{nameof(GetSubstituteForPartsOf)}/{nameof(Use)}/{nameof(UseCollection)}) should be used when testing to avoid confusion of which should be used.");
            }
            
            _diagnosticsHandler.AddDiagnosticMessagesForType(type, "Existing substitute found. Will use this!");
            return substituteTypeObject.Instance;
        }

        //Substitute needs creating
        _diagnosticsHandler.AddDiagnosticMessagesForType(type, "Substitute not found. Will create and store this for later use");
        var substituteInstance = actionCreateSubstitute();
        _ = _substituteTypeMap.TryAdd(type, new SubstituteTypeObject(substituteType, substituteInstance));

        return substituteInstance;
    }

    private bool TryGetService(Type serviceType, out SubstituteTypeObject mappedSubstituteTypeObject)
    {
        //Try get back according to the specific type give 
        if (!_substituteTypeMap.TryGetValue(serviceType, out mappedSubstituteTypeObject))
        {
            //If not found, check it isn't a enumerable collection created by this framework
            if (serviceType.IsCollection())
            {
                var underlyingCollectionType = serviceType.GetUnderlyingCollectionType();
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(underlyingCollectionType);

                return _substituteTypeMap.TryGetValue(enumerableType, out mappedSubstituteTypeObject);
            }

            //Nothing found
            return false;
        }

        //Found a type in the map
        return true;
    }

    private object CreateExceptionThrowingSubstituteOrThrow(Type constructorParameterType, Type diagnosticParameterType)
    {
        if (constructorParameterType.IsInterface)
        {
            return constructorParameterType.CreateExceptionThrowingSubstitute();
        }

        _diagnosticsHandler.AddDiagnosticMessagesForType(diagnosticParameterType, "When using behaviour 'Manual with Exceptions', only interfaces are usable with this behaviour");
        throw new AutoSubstituteException("Only interfaces are usable when using 'Manual with Exceptions' behaviour");
    }

    object? IServiceProvider.GetService(Type serviceType) => TryGetService(serviceType, out var mappedSubstituteTypeObject) ? 
        mappedSubstituteTypeObject.Instance : 
        null;
}