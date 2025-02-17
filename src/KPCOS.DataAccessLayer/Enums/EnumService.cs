namespace KPCOS.DataAccessLayer.Enums;

public enum EnumService
{
    M2,
    M3, 
    Unit
}

public class EnumServiceDetails
{
    public string Value;
    public string Name;
    public string Description;

    public EnumServiceDetails(string name, string description, string value)
    {
        Name = name;
        Description = description;
        Value = value;
    }

    public static readonly Dictionary<EnumService, EnumServiceDetails> EnumServiceMapping = new()
    {   
        { EnumService.M2, new EnumServiceDetails("M2", "Square meter", "1") },
        { EnumService.M3, new EnumServiceDetails("M3", "Cubic meter", "2") },
        { EnumService.Unit, new EnumServiceDetails("Unit", "Unit", "3") }
    };
}
