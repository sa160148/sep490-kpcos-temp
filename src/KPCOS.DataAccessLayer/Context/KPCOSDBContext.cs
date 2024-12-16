using KPCOS.DataAccessLayer.Context.DataGenerator;
using KPCOS.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KPCOS.DataAccessLayer.Context;

public partial class KPCOSDBContext : DbContext
{
    public KPCOSDBContext()
    {
    }
    
    public KPCOSDBContext(DbContextOptions<KPCOSDBContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(GetConnectionString() /*hardConn*/);

    string hardConn = "Host=localhost;Port=5432;Database=kpcos;Username=sa;Password=123@123Bb";
    string GetConnectionString()
    {
        IConfiguration builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        return builder["ConnectionStrings:Default"];
        ;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamptz")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("username");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");
            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });
        OnModelCreatingPartial(modelBuilder);
        new DBInitData(modelBuilder).Seeding();
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}