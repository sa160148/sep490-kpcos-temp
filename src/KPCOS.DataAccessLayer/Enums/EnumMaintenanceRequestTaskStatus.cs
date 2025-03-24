using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumMaintenanceRequestTaskStatus
{
    [EnumMember(Value = "OPENING")]
    OPENING,
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "PREVIEWING")]
    PREVIEWING,
    [EnumMember(Value = "DONE")]
    DONE
}
