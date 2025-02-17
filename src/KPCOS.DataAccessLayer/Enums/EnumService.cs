using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum EnumService
{
    [EnumMember(Value = "M2")]
    M2,
    [EnumMember(Value = "M3")]
    M3, 
    [EnumMember(Value = "UNIT")]
    Unit
}

