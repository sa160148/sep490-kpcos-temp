using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumFeedbackType
{
    [EnumMember(Value = "PROJECT")]
    PROJECT,
    [EnumMember(Value = "MAINTENANCE")]
    MAINTENANCE
}
