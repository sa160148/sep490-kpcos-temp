using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumDocStatus
{
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "ACTIVE")]
    ACTIVE,
    [EnumMember(Value = "CANCELLED")]
    CANCELLED,
}
