namespace NSubstitute.AutoSub.Classes.Enums;

/// <summary>
/// Ways that substitutes can be made
/// </summary>
internal enum SubstituteType
{
    /// <summary>
    /// Created via <see cref="AutoSubstitute.SubstituteFor{T}"/>
    /// </summary>
    For,
    
    /// <summary>
    /// Created via <see cref="AutoSubstitute.SubstituteForPartsOf{T}"/>
    /// </summary>
    ForPartsOf,
    
    /// <summary>
    /// Created via <see cref="AutoSubstitute.Use{T}"/> or <see cref="AutoSubstitute.UseCollection{T}"/>
    /// </summary>
    Manual
}