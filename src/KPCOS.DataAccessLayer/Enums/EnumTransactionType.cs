using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumTransactionType
{
    [EnumMember(Value = "PAYMENT_BATCH")]
    PAYMENT_BATCH,
    [EnumMember(Value = "MAINTENANCE_REQUEST")]
    MAINTENANCE_REQUEST,
}