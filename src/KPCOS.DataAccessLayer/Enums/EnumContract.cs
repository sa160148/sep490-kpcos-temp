using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum EnumContract
{
    [EnumMember(Value = "PROCESS")]
    PROCESS,
    
    [EnumMember(Value = "ACTIVE")]
    ACTIVE,
    
    [EnumMember(Value = "CANCEL")]
    CANCEL,
}