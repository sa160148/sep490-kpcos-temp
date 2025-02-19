using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum RoleEnum
{
    [EnumMember(Value = "ADMINISTRATOR")]
    ADMINISTRATOR,
    [EnumMember(Value = "CONSULTANT")]
    CONSULTANT,
    [EnumMember(Value = "DESIGNER")]
    DESIGNER,
    [EnumMember(Value = "MANAGER")]
    MANAGER,
    [EnumMember(Value = "CUSTOMER")]
    CUSTOMER,
    [EnumMember(Value = "CONSTRUCTOR")]
    CONSTRUCTOR,
    
}