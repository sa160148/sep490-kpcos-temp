using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumTransactionStatus
{
    [EnumMember(Value = "SUCCESSFUL")]
    SUCCESSFUL,
    [EnumMember(Value = "FAILED")]
    FAILED
}
