using KPCOS.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Context.DataGenerator;

public class DBInitData
{
    private readonly ModelBuilder _modelBuilder;

    public DBInitData(ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }

    public void Seeding()
    {
        var adRole = new Role
        {
            Id = new Guid(),
            Name = "ADMIN",
            Description = "Admin role",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _modelBuilder.Entity<Role>().HasData(
            adRole
        );

       
    }
}