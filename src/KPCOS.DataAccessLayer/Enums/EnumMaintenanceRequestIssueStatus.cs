using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumMaintenanceRequestIssueStatus
{
    [EnumMember(Value = "OPENING")]
    OPENING,
    [EnumMember(Value = "PROCESSING")]
    PROCESSING,
    [EnumMember(Value = "PREVIEWING")]
    PREVIEWING,
    [EnumMember(Value = "DONE")]
    DONE,
    [EnumMember(Value = "CANCELLED")]
    CANCELLED,
}
