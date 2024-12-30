using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

public enum TypeConversation
{
    [EnumMember(Value = "PRIVATE")]
    PRIVATE,
    [EnumMember(Value = "GROUP")]
    GROUP,
}