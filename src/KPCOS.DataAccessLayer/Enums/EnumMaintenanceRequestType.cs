using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumMaintenanceRequestType
{
    [EnumMember(Value = "SCHEDULED")]
    SCHEDULED,
    [EnumMember(Value = "UNSCHEDULED")]
    UNSCHEDULED,
    [EnumMember(Value = "OTHER")]
    OTHER
}
