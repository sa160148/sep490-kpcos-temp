using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumNotificationType
{
    [EnumMember(Value = "PROMOTION")]
    PROMOTION,
    [EnumMember(Value = "PROJECT")]
    PROJECT,
    [EnumMember(Value = "MAINTENANCE")]
    MAINTENANCE,
    [EnumMember(Value = "DESIGN")]
    DESIGN,
    [EnumMember(Value = "QUOTATION")]
    QUOTATION,
    [EnumMember(Value = "CONTRACT")]
    CONTRACT,
    [EnumMember(Value = "CONSTRUCTION")]
    CONSTRUCTION,
    [EnumMember(Value = "STAFF")]
    STAFF,
    [EnumMember(Value = "OTHER")]
    OTHER
}
