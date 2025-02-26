using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum EnumQuotationStatus
{
    [EnumMember(Value = "OPEN")]
    OPEN,
    [EnumMember(Value = "PREVIEW")]
    PREVIEW,
    [EnumMember(Value = "APPROVED")]
    APPROVED,
    [EnumMember(Value = "REJECTED")]
    REJECTED,
    [EnumMember(Value = "CANCELLED")]
    CANCELLED,
    [EnumMember(Value = "UPDATING")]
    UPDATING,
    [EnumMember(Value = "CONFIRMED")]
    CONFIRMED,
    
    
}