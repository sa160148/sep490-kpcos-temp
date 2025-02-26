using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumContractStatus
{
    [EnumMember(Value = "PROCESSING")]
    Processing,
    [EnumMember(Value = "ACTIVE")]
    Active,
    [EnumMember(Value = "INACTIVE")]
    Cancelled
}