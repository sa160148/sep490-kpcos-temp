using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumContractStatus
{
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "ACTIVE")]
    ACTIVE,
    [EnumMember(Value = "CANCELLED")]
    CANCELLED
}