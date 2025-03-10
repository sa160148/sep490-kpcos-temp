using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumBillType
{
    [EnumMember(Value = "250000")]
    HOA_DON_THANH_TOAN = 250000,
    [EnumMember(Value = "250006")]
    HOA_DON_DICH_VU = 250006
}
