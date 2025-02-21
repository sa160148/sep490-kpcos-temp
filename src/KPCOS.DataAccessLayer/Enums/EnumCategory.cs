using System.Runtime.Serialization;

namespace KPCOS.DataAccessLayer.Enums;

[DataContract]
public enum EnumCategory
{
    [EnumMember(Value = "PRELIMINARIES")]
    Preliminaries,
    [EnumMember(Value = "POND LAYOUT")]
    PondLayou,
    [EnumMember(Value = "PLUMBING WORKS")]
    PlumbingWorks,
    [EnumMember(Value = "POWER HOUSE")]
    PowerHouse,
    [EnumMember(Value = "WATER STORAGE TANK PLATFORM")]
    WaterStorageTankPlatform,
    [EnumMember(Value = "LANDSCAPING")]
    Landscaping,
    [EnumMember(Value = "CONTINGENCY")]
    Contingency
}

public class EnumCategoryDetails
{
    public int Value;
    public string Name;
    public string Description;
    
    public EnumCategoryDetails(string name, string description, int value)
    {
        Name = name;
        Description = description;
        Value = value;
    }
    
    public static readonly Dictionary<EnumCategory, EnumCategoryDetails> EnumCategoryMapping = new()
    {   
        { EnumCategory.Preliminaries, new EnumCategoryDetails("Preliminaries", "Preliminaries", 1) },
        { EnumCategory.PondLayou, new EnumCategoryDetails("PondLayout", "Pond Layout", 2) },
        { EnumCategory.PlumbingWorks, new EnumCategoryDetails("PlumbingWorks", "Plumbing Works", 3) },
        { EnumCategory.PowerHouse, new EnumCategoryDetails("PowerHouse", "Power House", 4) },
        { EnumCategory.WaterStorageTankPlatform, new EnumCategoryDetails("WaterStorageTankPlatform", "Water Storage Tank Platform", 5) },
        { EnumCategory.Landscaping, new EnumCategoryDetails("Landscaping", "Landscaping", 6) },
        { EnumCategory.Contingency, new EnumCategoryDetails("Contingency", "Contingency", 7) }
    };
    
}