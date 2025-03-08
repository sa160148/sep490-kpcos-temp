using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

/// <summary>
/// Represents the status of a construction item
/// </summary>
[DataContract]
public enum EnumConstructionItemStatus
{
    /// <summary>
    /// Initial status for new construction items
    /// </summary>
    [EnumMember(Value = "OPENING")]
    OPENING,
    
    /// <summary>
    /// Construction items that are currently in progress
    /// </summary>
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    
    /// <summary>
    /// Completed construction items
    /// </summary>
    [EnumMember(Value = "DONE")]
    DONE
}