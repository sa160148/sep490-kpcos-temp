using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumPaymentStatus
{
    [EnumMember(Value = "DEPOSIT")]
    DEPOSIT,
    [EnumMember(Value = "ACCEPTANCE")]  
    ACCEPTANCE,
    [EnumMember(Value = "PRE_CONSTRUCTING")]
    PRE_CONSTRUCTING,
    [EnumMember(Value = "CONSTRUCTING")]
    CONSTRUCTING
}
