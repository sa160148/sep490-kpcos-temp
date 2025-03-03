using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum EnumConstructionItem
{
    [EnumMember(Value = "NORMAL")]
    NORMAL,
    
    [EnumMember(Value = "SPECIAL")]
    SPECIAL,
}