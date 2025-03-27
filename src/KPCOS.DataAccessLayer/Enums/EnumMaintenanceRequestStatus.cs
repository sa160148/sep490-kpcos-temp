using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumMaintenanceRequestStatus
{
    [EnumMember(Value = "OPENING")]
    OPENING,
    [EnumMember(Value = "CANCELLED")]
    CANCELLED,
    [EnumMember(Value = "REQUESTING")]
    REQUESTING,
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "DONE")]
    DONE
}
