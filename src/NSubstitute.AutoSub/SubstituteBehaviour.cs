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
    /// means if a explicit dependency mock has not been created before a method that uses the dependency,
    /// then it will throw an exception.
    /// </summary>
    ManualWithNulls = 1,
    
    ManualWithExceptions = 3
}