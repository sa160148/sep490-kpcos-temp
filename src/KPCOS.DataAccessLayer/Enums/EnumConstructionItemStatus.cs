using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumConstructionItemStatus
{
    [EnumMember(Value = "OPENING")]
    OPENING,
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "DONE")]
    DONE
}