using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumProjectStatus
{
    [EnumMember(Value = "REQUESTING")]
    REQUESTING,
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "DESIGNING")]
    DESIGNING,
    [EnumMember(Value = "CONSTRUCTING")]
    CONSTRUCTING,
    [EnumMember(Value = "FINISHED")]
    FINISHED,
}