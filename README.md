# NSubstitute.AutoSubstitute
This package is an automocking container for NSubsitute. The purpose of this project is to try and de-couple unit tests from the constructors of your systems under test making them less brittle as changes are made throughout your codebase but still enabling tests to pass/fail as expected.

Inspiration was taken from [AutoMocker](https://github.com/moq/Moq.AutoMocker) library which offers similar functionality as this library, but for Moq. It has been born out of using [NSubstitute](https://nsubstitute.github.io/) and wanting a similar workflow to be available.

## Usage
### Simplest Form
Here is a simple example to get you started on the right path:
```csharp
//Create an instance of AutoSubstitute to create systems to test from
var autoSubstitute = new AutoSubstitute();

//Get a mock from AutoSubstitute (you can get this before or after an instance is created)
var dependencyService = autoSubstitute.SubstituteFor<ITextGenerationDependency>();

//From here you can use NSubstitute as usual
dependencyService
    .GenerateText()
    .Returns("Test Text");

//Create a instance of the system that you wish to test
var sut = autoSubstitute.CreateInstance<ContentGenerationService>();

//Call the method using the dependency you have mocked
var result = sut.CreateContent();

//Verify with your favourite framework!
Assert.Equal("Test Text", result);
```

### Collections
You might come across a scenario where a collection is being used as a dependency. In which case, you have several options to potentially use:
```csharp
//Create the AutoSubsitute instance
var autoSubstitute = new AutoSubstitute();

//-- Option #1: If you only want to test one instance
//When a instance is created, the single instance will be wrapped in a collection and injected as a dependency
var multipleDependency = autoSubstitute.SubstituteFor<IMultipleDependency>();

//Mock as you wish
multipleDependency
    .GenerateText()
    .Returns("Test Text");

//-- Option #2: If you have multiple dependencies
//You can create a substitute that will not be cached by AutoSubstitute and just create a plain old substitute/mock
var multipleDependency1 = autoSubstitute.SubstituteFor<IMultipleDependency>(noCache: true);
var multipleDependency2 = autoSubstitute.SubstituteForNoCache<IMultipleDependency>();

//Mock as you wish
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
Built into AutoSubstitute is 3 different behaviour types you can use. These behaviours do have some common traits:
- Mocks that are requested via `SubstituteFor` or `SubstitutePartsFor` will automatically be cached so that they can be used when a system under test is constructed via `CreateInstance`
- All constructors will be analysed and checked in order of the constructors with the most parameters
- Constructor parameters will be assessed if they are accessible and if mocks can be made for them. If at any point a parameter is deemed unsuitable, for example lack of access, the next constructor is checked

The specifics of each behaviour are as follows:
- **Automatic (Default)**
  - This is the `default` behaviour of this framework. 
  - Any dependency that is not cached will be automatically generated and then cached afterwards in case extra mock behahviour might want to be configured after calling `CreateInstance`
  - Dependencies that are interfaces will use `Substitute.For` and classes will use `Substitute.ForPartsOf`
- **Manual with Nulls**
  - Is enabled via passing in a parameter via the constructor for `AutoSubstitute`:
    ```csharp
    var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithNulls);
    ```
  - Any dependency that is not cached will not be generated and `null` will be passed through for any dependency that hasn't been requested prior to calling `CreateInstance`
- **Manual with Exceptions**
  - Is enabled via passing in a parameter via the constructor for `AutoSubstitute`:
    ```csharp
    var autoSubstitute = new AutoSubstitute(SubstituteBehaviour.ManualWithExceptions);
    ```
  - Any dependency that is not cached will be generated as a "exception throwing mock". What this means is, a mock instance will be created but every property/method will throw an exception when called. However it will give a informative message as to what exactly hasn't been mocked to the user, e.g.
    > Mock has not been configured for 'ITextGenerationDependency' when method 'Generate' was invoked. When using a 'Manual' behaviour, the mock must be created before 'CreateInstance' is called.
  - This will only work with **interface** dependencies. This is because in the case of a class, it is difficult to assume what should and shouldn't be mocked to throw a exception.

### Received Helpers
Helpers has been provided to be able to do verifications on dependencies being invoked. This is just syntactic sugar to try simplify the process of verification.

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
- No Cache
  - You can create subsitute instances without having `AutoSubstitute` track them. To do this, pass in the `noCache` flag when creating a substitute/mock or use `SubstituteForNoCache`
    ```csharp
    //Create the AutoSubsitute instance
    var autoSubstitute = new AutoSubstitute(usePrivateConstructors: true);

    //-- Option #1: Via method
    autoSubstitute.SubstituteForNoCache<ITextGenerationDependency>();

    //-- Option #2: Via parameter
    autoSubstitute.SubstituteFor<ITextGenerationDependency>(noCache: true);
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
AutoSubstitute and its tests have been developed using Rider however should be compabile with Visual Studio. The solution can be found in the root of this repository.

## Acknowledgements
- [NSubstitute](https://nsubstitute.github.io/): A friendly substitute for .NET mocking libraries. This framework is built around this framework as a base for testing and wouldn't exist if not for it.
- [AutoMocker](https://github.com/moq/Moq.AutoMocker): Inspiration for this project. Great framework to be able to achieve decoupling from your constructor parameters in your tests and making them less brittle.
- [XUnit](https://github.com/xunit/xunit): Used to be able to test everything works as expected.