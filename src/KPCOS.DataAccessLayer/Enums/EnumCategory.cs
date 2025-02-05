namespace KPCOS.DataAccessLayer.Enums;

public enum EnumCategory
{
    Preliminaries = 1,
    PondLayou = 2,
    PlumbingWorks = 3,
    PowerHouse = 4,
    WaterStorageTankPlatform = 5,
    Landscaping = 6,
    Contingency = 7
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
        { EnumCategory.PondLayou, new EnumCategoryDetails("PondLayou", "Pond Layou", 2) },
        { EnumCategory.PlumbingWorks, new EnumCategoryDetails("PlumbingWorks", "Plumbing Works", 3) },
        { EnumCategory.PowerHouse, new EnumCategoryDetails("PowerHouse", "Power House", 4) },
        { EnumCategory.WaterStorageTankPlatform, new EnumCategoryDetails("WaterStorageTankPlatform", "Water Storage Tank Platform", 5) },
        { EnumCategory.Landscaping, new EnumCategoryDetails("Landscaping", "Landscaping", 6) },
        { EnumCategory.Contingency, new EnumCategoryDetails("Contingency", "Contingency", 7) }
    };
    
}