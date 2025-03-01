using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumDesignStatus
{
    [EnumMember(Value = "OPENING")]
    OPENING,
    [EnumMember(Value = "REJECTED")]
    REJECTED,
    [EnumMember(Value = "PREVIEWING")]
    PREVIEWING,
    [EnumMember(Value = "EDITING")]
    EDITING,
    [EnumMember(Value = "CONFIRMED")]
    CONFIRMED,
}