using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum RoleEnum
{
    [EnumMember(Value = "ADMIN")]
    ADMIN,
    [EnumMember(Value = "GUEST")]
    GUEST,
    [EnumMember(Value = "CONSULTOR")]
    CONSULTOR,
    [EnumMember(Value = "DESIGNER")]
    DESIGNER,
    [EnumMember(Value = "MANAGER")]
    MANAGER,
    [EnumMember(Value = "CUSTOMER")]
    CUSTOMER,
    [EnumMember(Value = "CONSTRUCTOR")]
    CONSTRUCTOR,
    
}