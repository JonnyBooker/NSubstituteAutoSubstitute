namespace NSubstitute.AutoSub;

/// <summary>
/// Defines how substitutes are handled when created during instance creation
/// </summary>
public enum SubstituteBehaviour
{
    /// <summary>
    /// All instances created will have their dependencies created automatically. 
    /// </summary>
    Automatic = 0,
    
    /// <summary>
    /// Will not automatically create any dependencies when searching for suitable constructors. This
    /// means if a explicit dependency has not been created before a method is called that uses the dependency,
    /// then it will send through NULL
    /// </summary>
    ManualWithNulls = 1,
    
    /// <summary>
    /// When creating instances using this behaviour, when a explicit dependency has not been used or
    /// created before a that is called that uses the dependency, that a explicit exception will be thrown which
    /// will give a "nice" error saying a dependency hasn't been mocked before its use in a test.
    /// </summary>
    ManualWithExceptions = 3
}