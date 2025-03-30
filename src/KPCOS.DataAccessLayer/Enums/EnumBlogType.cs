namespace KPCOS.DataAccessLayer.Enums;

/// <summary>
/// Represents the different types of blog posts in the koi pond construction system
/// </summary>
public enum EnumBlogType
{
    /// <summary>
    /// Blog post related to a specific project
    /// </summary>
    /// <remarks>
    /// Used for showcasing completed or ongoing koi pond construction projects
    /// </remarks>
    PROJECT,
    
    /// <summary>
    /// Blog post related to a maintenance package
    /// </summary>
    /// <remarks>
    /// Used for explaining maintenance services for koi ponds
    /// </remarks>
    MAINTENANCE_PACKAGE,
    
    /// <summary>
    /// Blog post related to a construction package
    /// </summary>
    /// <remarks>
    /// Used for explaining various pond construction packages and offerings
    /// </remarks>
    PACKAGE,
    
    /// <summary>
    /// Blog post related to an event
    /// </summary>
    /// <remarks>
    /// Used for koi pond-related events, exhibitions, or workshops
    /// </remarks>
    EVENT,
    
    /// <summary>
    /// Blog post of other miscellaneous types
    /// </summary>
    /// <remarks>
    /// Used for general informational content about koi ponds when not related to specific entities
    /// </remarks>
    OTHER
}
