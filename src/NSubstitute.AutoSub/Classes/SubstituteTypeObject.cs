using NSubstitute.AutoSub.Classes.Enums;

namespace NSubstitute.AutoSub.Classes;

/// <summary>
/// Used for tracking a substitute instance and how it was created
/// </summary>
internal class SubstituteTypeObject
{
    public SubstituteTypeObject(SubstituteType substituteType, object instance)
    {
        SubstituteType = substituteType;
        Instance = instance;
    }

    /// <summary>
    /// The method in which this substitute was created
    /// </summary>
    public SubstituteType SubstituteType { get; }

    /// <summary>
    /// The substituted instance
    /// </summary>
    public object Instance { get; }
}