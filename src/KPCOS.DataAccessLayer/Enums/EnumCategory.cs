using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumCategory
{
    [EnumMember(Value = "PRELIMINARIES")]
    Preliminaries,
    [EnumMember(Value = "POND_LAYOUT")]
    PondLayout,
    [EnumMember(Value = "PLUMBING_WORKS")]
    PlumbingWorks,
    [EnumMember(Value = "POWER_HOUSE")]
    PowerHouse,
    [EnumMember(Value = "WATER_STORAGE_TANK_PLATFORM")]
    WaterStorageTankPlatform,
    [EnumMember(Value = "LANDSCAPING")]
    Landscaping,
    [EnumMember(Value = "CONTINGENCY")]
    Contingency
}

