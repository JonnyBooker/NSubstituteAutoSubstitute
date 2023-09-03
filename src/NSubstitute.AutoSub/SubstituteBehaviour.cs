namespace NSubstitute.AutoSub;

public enum SubstituteBehaviour
{
    /// <summary>
    /// Use <see cref="Substitute.For{T}"/> for creating any substitutes. All instances
    /// created will have their dependencies created automatically
    /// </summary>
    LooseFull = 0,
    
    /// <summary>
    /// Use <see cref="Substitute.ForPartsOf{T}"/> for creating any substitutes. All instances
    /// created will have their dependencies created automatically
    /// </summary>
    LooseParts = 1,
    
    /// <summary>
    /// Will not automatically create any dependencies of a class and will return null unless
    /// behaviours of a class are explicitly defined 
    /// </summary>
    Strict = 2
}