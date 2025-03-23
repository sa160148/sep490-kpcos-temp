using System;

namespace KPCOS.DataAccessLayer.Entities;

public class DocType
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Doc> Docs { get; set; } = new List<Doc>();
}
