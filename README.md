# NSubstitute.AutoSubstitute
[![Continuous Build Workflow](https://github.com/JonnyBooker/NSubstitute.AutoSubstitute/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/JonnyBooker/NSubstitute.AutoSubstitute/actions/workflows/dotnetcore.yml) [![NuGet Status](https://img.shields.io/nuget/v/NSubstituteAutoSubstitute.svg?style=flat)](https://www.nuget.org/packages/NSubstituteAutoSubstitute)

This package is an auto-substitute/mock container for [NSubstitute](https://nsubstitute.github.io/). The purpose of this project is to try and decouple unit tests from the constructors of your systems under test making them less brittle as changes are made throughout your codebase but still enabling tests to pass/fail as expected.

Inspiration was taken from [AutoMocker](https://github.com/moq/Moq.AutoMocker) library which offers similar functionality to this library, but for Moq. It has been born out of using [NSubstitute](https://nsubstitute.github.io/) and wanting a similar workflow to be available.

## Usage
### Simplest Form
Here is a simple example to get you started on the right path:
```csharp
//Create an instance of AutoSubstitute to create systems to test from
var autoSubstitute = new AutoSubstitute();

//Get a mock from AutoSubstitute (you can get this before or after an instance is created)
var dependencyService = autoSubstitute.GetSubstituteFor<ITextGenerationDependency>();

//From here you can use NSubstitute as usual
dependencyService
    .GenerateText()
    .Returns("Test Text");

//Create a instance of the system that you wish to test (system under test)
var sut = autoSubstitute.CreateInstance<ContentGenerationService>();

//Call the method using the dependency you have mocked
var result = sut.CreateContent();

//Verify with your favourite testing framework!
Assert.Equal("Test Text", result);
```

### Methods For Mocking
You can create substitutes using `For` and `ForPartsOf` in AutoSubstitute. Please follow the [warning from NSubstitutes](https://nsubstitute.github.io/help/creating-a-substitute/#substituting-infrequently-and-carefully-for-classes) when using classes not interfaces.
```csharp
//NSubstitute 'For'
var dependency = Substitute.For<IDependency>();
var dependency = AutoSubstitute.GetSubstituteFor<IDependency>();

//NSubstitute 'ForPartsOf'
var dependency = Substitute.ForPartsOf<IDependency>();
var dependency = AutoSubstitute.GetSubstituteForPartsOf<IDependency>();
```

### Tracking Dependencies
Unless the behaviour of `AutoSubstitute` is changed (see [behaviours](#behaviours)), the first time that a dependency is required/interacted with, it is created and subsequently tracked by `AutoSubstitute`. This means that anytime in future when you ask `AutoSubstitute` for a dependency, it will return the same instance everytime. 

This instance is created when:
- `GetSubstituteFor`/`GetSubstituteForPartsOf` is invoked prior to calling `CreateInstance` for a system you are attempting to test 
- `CreateInstance` is invoked prior to calling `GetSubstituteFor`/`GetSubstituteForPartsOf` and it is required in the constructor of the system you wish to test

This ensures that when it comes time to `CreateInstance` being invoked, the dependencies that you have mocked/stubbed are the same ones that are injected into the constructors of any systems you are testing.

You can create substitute instances without having `AutoSubstitute` track them. To do this, use the following methods:
```csharp
//Create the AutoSubsitute instance
var autoSubstitute = new AutoSubstitute();

//Call one of the "NoTracking" methods
autoSubstitute.GetSubstituteForNoTracking<ITextGenerationDependency>();
autoSubstitute.GetSubstituteForPartsOfNoTracking<TextGenerationDependency>();
```

**Important Note:** Please note that should you change between using `GetSubstituteFor`/ `GetSubstituteForPartsOf`, a exception will be thrown. This has been done to ensure the substitute created remains the same throughout its lifetime. When using
`Use`/`UseCollection`, the instance created here will take precedence over any substitute previously created and will always be returned when using `GetSubstituteFor`/ `GetSubstituteForPartsOf`. 

### Mocking Collections
You might come across a scenario where a collection is being used as a dependency. In which case, you have several options to potentially use:
```csharp
//Create the AutoSubsitute instance
var autoSubstitute = new AutoSubstitute();

//-- Option #1
//If you only intend to use one mock as part of the collection of dependencies, you can use AutoSubstitute as you normally would for mocking a single dependency
//When the collection of dependencies is created, the single mocked dependency will be wrapped in a collection and injected
var multipleDependency = autoSubstitute.GetSubstituteFor<IMultipleDependency>();

//Mock as you wish
multipleDependency
    .GenerateText()
    .Returns("Test Text");

//-- Option #2
//If you want to have multiple mock dependencies injected. You can create a substitute that will not be tracked by AutoSubstitute and just create a plain old substitute/mock
var multipleDependency1 = autoSubstitute.GetSubstituteFor<IMultipleDependency>(noTracking: true);
var multipleDependency2 = autoSubstitute.GetSubstituteForNoTracking<IMultipleDependency>();

//Mock each dependency as you wish
multipleDependency1
    .GenerateText()
    .Returns("Test");

multipleDependency1
    .GenerateText()
    .Returns("Text");

//Tell AutoSubstitute to use both these dependencies whenever a collection is found as construction parameter. Multiple collection types are supported.
autoSubstitute.UseCollection(instance1, instance2);

//Carry on as normal...
var sut = autoSubstitute.CreateInstance<SimpleSystemUnderTest>();
var result = sut.CombineMultipleDependencyResults();

//Assert!
Assert.Equal("Test Text", result);
```

Classes used in above example:
```csharp
//System Under Test
public class SimpleSystemUnderTest
{
    private readonly IEnumerable<IMultipleDependency> _multipleDependencies;

    public SimpleSystemUnderTest(IEnumerable<IMultipleDependency> multipleDependencies)
    {
        _multipleDependencies = multipleDependencies;
    }

    public string CombineMultipleDependencyResults()
    {
        return string.Join(" ", _multipleDependencies.Select(d => d.GenerateText()));
    }
}

//Dependency Example
public interface IMultipleDependency
{
    string GenerateText();
}
```

### Behaviours
Built into AutoSubstitute are 3 different behaviour types you can use. These behaviours do have some common traits:
- All constructors will be analysed and checked in order of the most parameters/arguments
- Constructor parameters will be assessed if they are accessible and if mocks/substitutes can be created. If at any point a parameter is deemed unsuitable, for example lack of access, the next constructor is checked.
- If not suitable constructor is found, a `AutoSubstituteException` will be thrown

The specifics of each behaviour are as follows:
- **Automatic (Default)**
  - This is the `default` behaviour of this framework. 
  - Any mock/substitute that is used, whether requested via `GetSubstituteFor`/`SubstitutePartsFor` or if it created as part of a constructor when `CreateInstance` is called will only be created once.
  - Automatically created dependencies will always use `Substitute.For` 
- **Manual with Nulls**
  - Enabled via passing in `SubstituteBehaviour.ManualWithNulls` via the constructor for `AutoSubstitute`:

    ```csharp
    var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);
    ```
  - Any dependency that is not created/tracked prior to calling `CreateInstance` will not be generated and `null` will be passed through to the constructor of the instance being tested
- **Manual with Exceptions**
  - Enabled via passing in `SubstituteBehaviour.ManualWithExceptions` via the constructor for `AutoSubstitute`:

    ```csharp
    var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
    ```
  - Any dependency that is not created/tracked prior to calling `CreateInstance` will be created as a "exception throwing mock". What this means is, a mock instance will be created but every property/method will throw an exception when called. However it will give a informative message as to what exactly hasn't been mocked to the user, e.g.
    > Mock has not been configured for 'ITextGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.
  - This will only work with **interface** dependencies. This is because in the case of a class, it is difficult to assume what should and shouldn't be mocked to throw a exception.
  - If a dependency is requested via `GetSubstituteFor`/`SubstitutePartsFor` then the exception throwing mock will be replaced and it will be tracked for future use.

### Received Helpers
Helpers have been provided to be able to carry out verifications to ensure dependency invocations have or haven't taken place. This is just syntactic sugar to try simplify the process of verification but you are free to just use NSubstitute `Received` style extensions if you wish.

For example using the classes from the basic usage above:
```csharp
//Create the AutoSubsitute instance
var autoSubstitute = new AutoSubstitute();

//Create an instance and invoke a method
var sut = autoSubstitute.CreateInstance<ContentGenereationService>();
_ = sut.CreateContent();

//-- Option #1: Received Once
AutoSubstitute
    .ReceivedOnce<ITextGenerationDependency>(x => x.GenerateText());

//-- Option #2: At least once
AutoSubstitute
    .ReceivedAtLeastOnce<ITextGenerationDependency>(x => x.GenerateText());

//-- Option #3: Specified amount of times
AutoSubstitute
    .ReceivedTimes<ITextGenerationDependency>(x => x.GenerateText(), 1);

//-- Option #4: Never received
AutoSubstitute
    .DidNotReceive<ITextGenerationDependency>(x => x.GenerateText());
```

## Extra Options/Features
- Private Constructors
  - If you need to be able to access `private` constructors, an extra parameter can be passed into the constructor of `AutoSubstitute` via a flag: `usePrivateConstructors`
    ```csharp
    var autoSubstitute = new AutoSubstitute(usePrivateConstructors: true);
    ```
- Service Provider
  - `AutoSubstitute` implements `IServiceProvider` interface so it can be used as as a dependency injection container
- Diagnostics
  - All behaviours/checks/validations are logged to the `DiagnosticsHandler` instance on each `AutoSubstitute` instance. This will log things like:
    - Reasons why constructors aren't selected
    - When mocks/substitutes are created
    - More detailed errors
  - Events can also be subscribed to if you should so wish:
    ```csharp
    //Example Diagnostic Log Created
    autoSubstitute.DiagnosticsHandler.DiagnosticLogAdded += (_, args) =>
    {
        messages.Add((args.Type, args.Message));
    };
    ```

## Building
AutoSubstitute and its supporting tests have been developed using [Rider](https://www.jetbrains.com/rider/), however it should be compabile with Visual Studio. The solution file can be found in the root of this repository.

## Acknowledgements
- [NSubstitute](https://nsubstitute.github.io/): A friendly substitute for .NET mocking libraries. This framework is built around NSubstitute as a base for testing and wouldn't exist if not for it.
- [AutoMocker](https://github.com/moq/Moq.AutoMocker): Inspiration for this project. Great framework to be able to achieve decoupling from your constructor parameters in your tests and making them less brittle.
- [XUnit](https://github.com/xunit/xunit): Used to be able to test everything works as expected.