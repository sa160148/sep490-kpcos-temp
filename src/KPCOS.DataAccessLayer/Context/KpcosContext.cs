using KPCOS.Common.Utilities;
using KPCOS.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPCOS.DataAccessLayer.Context;

public partial class KpcosContext : DbContext
{
    public KpcosContext()
    {
    }

    public KpcosContext(DbContextOptions<KpcosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ConstructionItem> ConstructionItems { get; set; }

    public virtual DbSet<ConstructionTask> ConstructionTasks { get; set; }

    public virtual DbSet<ConstructionTemplate> ConstructionTemplates { get; set; }

    public virtual DbSet<ConstructionTemplateItem> ConstructionTemplateItems { get; set; }

    public virtual DbSet<ConstructionTemplateTask> ConstructionTemplateTasks { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Design> Designs { get; set; }

    public virtual DbSet<DesignImage> DesignImages { get; set; }

    public virtual DbSet<Doc> Docs { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<MaintenanceItem> MaintenanceItems { get; set; }

    public virtual DbSet<MaintenancePackage> MaintenancePackages { get; set; }

    public virtual DbSet<MaintenancePackageItem> MaintenancePackageItems { get; set; }

    public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

    public virtual DbSet<MaintenanceRequestTask> MaintenanceRequestTasks { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PackageDetail> PackageDetails { get; set; }

    public virtual DbSet<PackageItem> PackageItems { get; set; }

    public virtual DbSet<PaymentBatch> PaymentBatches { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectStaff> ProjectStaffs { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Quotation> Quotations { get; set; }

    public virtual DbSet<QuotationDetail> QuotationDetails { get; set; }

    public virtual DbSet<QuotationEquipment> QuotationEquipments { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ProjectIssue> ProjectIssues { get; set; }
    
    public virtual DbSet<MaintenanceStaff> MaintenanceStaffs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(GlobalUtility.GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConstructionItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("construction_item_pkey");

            entity.ToTable("construction_item");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActualAt).HasColumnName("actual_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstimateAt).HasColumnName("estimate_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.IsPayment)
                .HasDefaultValue(false)
                .HasColumnName("is_payment");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.ConstructionItems)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("construction_item_project_id_fkey");
        });

        modelBuilder.Entity<ConstructionTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("construction_task_pkey");

            entity.ToTable("construction_task");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ConstructionItemId).HasColumnName("construction_item_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeadlineAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deadline_at");
            entity.Property(e => e.DeadlineActualAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deadline_actual_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ConstructionItem).WithMany(p => p.ConstructionTasks)
                .HasForeignKey(d => d.ConstructionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("construction_task_construction_item_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ConstructionTasks)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("construction_task_staff_id_fkey");
        });

        modelBuilder.Entity<ConstructionTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("construction_template_pkey");

            entity.ToTable("construction_template");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ConstructionTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("construction_template_item_pkey");

            entity.ToTable("construction_template_item");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Idparent).HasColumnName("idparent");
            entity.Property(e => e.Duration)
                .HasDefaultValue(0)
                .HasColumnName("duration");
            entity.Property(e => e.Idtemplate).HasColumnName("idtemplate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdparentNavigation).WithMany(p => p.InverseIdparentNavigation)
                .HasForeignKey(d => d.Idparent)
                .HasConstraintName("construction_template_item_idparent_fkey");

            entity.HasOne(d => d.IdtemplateNavigation).WithMany(p => p.ConstructionTemplateItems)
                .HasForeignKey(d => d.Idtemplate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("construction_template_item_idtemplate_fkey");
        });

        modelBuilder.Entity<ConstructionTemplateTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("construction_template_task_pkey");

            entity.ToTable("construction_template_task");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Idtemplateitem).HasColumnName("idtemplateitem");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdtemplateitemNavigation).WithMany(p => p.ConstructionTemplateTasks)
                .HasForeignKey(d => d.Idtemplateitem)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("construction_template_task_idtemplateitem_fkey");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contract_pkey");

            entity.ToTable("contract");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContractValue).HasColumnName("contract_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(255)
                .HasColumnName("customer_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.Project).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contract_project_id_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customer_pkey");

            entity.ToTable("customer");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Dob)
                .HasDefaultValueSql("'2000-01-01'::date")
                .HasColumnName("dob");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Point)
                .HasDefaultValue(0)
                .HasColumnName("point");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Customers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_user_id_fkey");
        });

        modelBuilder.Entity<Design>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("design_pkey");

            entity.ToTable("design");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPublic)
                .HasDefaultValue(false)
                .HasColumnName("is_public");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasOne(d => d.Project).WithMany(p => p.Designs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("design_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.Designs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("design_staff_id_fkey");
        });

        modelBuilder.Entity<DesignImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("design_image_pkey");

            entity.ToTable("design_image");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.DesignId).HasColumnName("design_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Design).WithMany(p => p.DesignImages)
                .HasForeignKey(d => d.DesignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("design_image_design_id_fkey");
        });

        modelBuilder.Entity<DocType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doc_type_pkey");

            entity.ToTable("doc_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Doc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("docs_pkey");

            entity.ToTable("docs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.DocTypeId).HasColumnName("doc_type_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");

            entity.HasOne(d => d.Project).WithMany(p => p.Docs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("docs_project_id_fkey");

            entity.HasOne(d => d.DocType).WithMany(p => p.Docs)
                .HasForeignKey(d => d.DocTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("docs_doc_type_id_fkey");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("equipment_pkey");

            entity.ToTable("equipment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MaintenanceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_item_pkey");

            entity.ToTable("maintenance_item");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MaintenancePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_package_pkey");

            entity.ToTable("maintenance_package");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MaintenancePackageItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_package_item_pkey");

            entity.ToTable("maintenance_package_item");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaintenanceItemId).HasColumnName("maintenance_item_id");
            entity.Property(e => e.MaintenancePackageId).HasColumnName("maintenance_package_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.MaintenanceItem).WithMany(p => p.MaintenancePackageItems)
                .HasForeignKey(d => d.MaintenanceItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_package_item_maintenance_item_id_fkey");

            entity.HasOne(d => d.MaintenancePackage).WithMany(p => p.MaintenancePackageItems)
                .HasForeignKey(d => d.MaintenancePackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_package_item_maintenance_package_id_fkey");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_request_pkey");

            entity.ToTable("maintenance_request");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.Depth).HasColumnName("depth");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.TotalValue).HasColumnName("total_value");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.IsPaid).HasColumnName("is_paid");
            entity.Property(e => e.MaintenancePackageId).HasColumnName("maintenance_package_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_request_customer_id_fkey");

            entity.HasOne(d => d.MaintenancePackage).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.MaintenancePackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_request_maintenance_package_id_fkey");
        });

        modelBuilder.Entity<MaintenanceStaff>(entity =>
        {
            entity.HasKey(e => new { e.MaintenanceRequestTaskId, e.StaffId }).HasName("maintenance_staff_pkey");

            entity.ToTable("maintenance_staff");

            entity.Property(e => e.MaintenanceRequestTaskId).HasColumnName("maintenance_request_task_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.MaintenanceRequestTask).WithMany(p => p.MaintenanceStaffs)
                    .HasForeignKey(d => d.MaintenanceRequestTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_staff_maintenance_request_task_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.MaintenanceStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_staff_staff_id_fkey");
        });

        modelBuilder.Entity<MaintenanceRequestTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_request_task_pkey");

            entity.ToTable("maintenance_request_task");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.EstimateAt).HasColumnName("estimate_at");
            entity.Property(e => e.MaintenanceRequestId).HasColumnName("maintenance_request_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.MaintenanceItemId).HasColumnName("maintenance_item_id");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.MaintenanceRequest).WithMany(p => p.MaintenanceRequestTasks)
                .HasForeignKey(d => d.MaintenanceRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_request_task_maintenance_request_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.MaintenanceRequestTasks)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_request_task_staff_id_fkey");

            entity.HasOne(d => d.MaintenanceItem).WithMany(p => p.MaintenanceRequestTasks)
                .HasForeignKey(d => d.MaintenanceItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("maintenance_request_task_maintenance_item_id_fkey");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_pkey");

            entity.ToTable("package");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PackageDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_detail_pkey");

            entity.ToTable("package_detail");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.PackageItemId).HasColumnName("package_item_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageDetails)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("package_detail_package_id_fkey");

            entity.HasOne(d => d.PackageItem).WithMany(p => p.PackageDetails)
                .HasForeignKey(d => d.PackageItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("package_detail_package_item_id_fkey");
        });

        modelBuilder.Entity<PackageItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("package_item_pkey");

            entity.ToTable("package_item");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PaymentBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_batch_pkey");

            entity.ToTable("payment_batch");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ConstructionItemId).HasColumnName("construction_item_id");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPaid)
                .HasDefaultValue(false)
                .HasColumnName("is_paid");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Percents).HasColumnName("percents");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.PaymentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("payment_at");
            entity.Property(e => e.TotalValue).HasColumnName("total_value");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Contract).WithMany(p => p.PaymentBatches)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payment_batch_contract_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_pkey");

            entity.ToTable("project");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(255)
                .HasColumnName("customer_name");
            entity.Property(e => e.Depth).HasColumnName("depth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Templatedesignid).HasColumnName("templatedesignid");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_customer_id_fkey");

            entity.HasOne(d => d.Package).WithMany(p => p.Projects)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_package_id_fkey");
        });

        modelBuilder.Entity<ProjectStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_staff_pkey");

            entity.ToTable("project_staff");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectStaffs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_staff_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_staff_staff_id_fkey");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("promotion_pkey");

            entity.ToTable("promotion");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Exptime).HasColumnName("exptime");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Starttime).HasColumnName("starttime");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quotation_pkey");

            entity.ToTable("quotation");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Idtemplate).HasColumnName("idtemplate");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasOne(d => d.IdtemplateNavigation).WithMany(p => p.Quotations)
                .HasForeignKey(d => d.Idtemplate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_idtemplate_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Quotations)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_project_id_fkey");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Quotations)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("quotation_promotion_id_fkey");
        });

        modelBuilder.Entity<QuotationDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quotation_detail_pkey");

            entity.ToTable("quotation_detail");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Quotation).WithMany(p => p.QuotationDetails)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_detail_quotation_id_fkey");

            entity.HasOne(d => d.Service).WithMany(p => p.QuotationDetails)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_detail_service_id_fkey");
        });

        modelBuilder.Entity<QuotationEquipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quotation_equipment_pkey");

            entity.ToTable("quotation_equipment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.QuotationId).HasColumnName("quotation_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Equipment).WithMany(p => p.QuotationEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_equipment_equipment_id_fkey");

            entity.HasOne(d => d.Quotation).WithMany(p => p.QuotationEquipments)
                .HasForeignKey(d => d.QuotationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quotation_equipment_quotation_id_fkey");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("service_pkey");

            entity.ToTable("service");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasMaxLength(255)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("staff_pkey");

            entity.ToTable("staff");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Position)
                .HasMaxLength(255)
                .HasColumnName("position");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Staff)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("staff_user_id_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transaction_pkey");

            entity.ToTable("transaction");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transaction_customer_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasDefaultValueSql("'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTaotZTcu1CLMGOJMDl-f_LYBECs7tqwhgpXA&s'::character varying")
                .HasColumnName("avatar");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<IssueType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("issue_type_pkey");

            entity.ToTable("issue_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProjectIssue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_issue_pkey");

            entity.ToTable("project_issue");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Cause)
                .HasMaxLength(255)
                .HasColumnName("cause");
            entity.Property(e => e.Solution)
                .HasMaxLength(255)
                .HasColumnName("solution");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.IssueImage)
                .HasMaxLength(255)
                .HasColumnName("issue_image");
            entity.Property(e => e.ConfirmImage)
                .HasMaxLength(255)
                .HasColumnName("confirm_image");
            entity.Property(e => e.ActualAt)
                .HasColumnName("actual_at");
            entity.Property(e => e.EstimateAt)
                .HasColumnName("estimate_at");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.IssueTypeId)
                .HasColumnName("issue_type_id");
            entity.Property(e => e.ConstructionItemId)
                .HasColumnName("construction_item_id");
            entity.Property(e => e.StaffId)
                .HasColumnName("staff_id");

            entity.HasOne(d => d.IssueType)
                .WithMany(p => p.ProjectIssues)
                .HasForeignKey(d => d.IssueTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_issue_issue_type_id_fkey");

            entity.HasOne(d => d.ConstructionItem)
                .WithMany(p => p.ProjectIssues)
                .HasForeignKey(d => d.ConstructionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_issue_construction_item_id_fkey");

            entity.HasOne(d => d.Staff)
                .WithMany(p => p.ProjectIssues)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_issue_staff_id_fkey");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("feedback_pkey");

            entity.ToTable("feedback");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("timezone('Asia/Bangkok'::text, now())")
                .HasColumnName("updated_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Rating)
                .HasColumnName("rating");
            entity.Property(e => e.No)
                .HasColumnName("no");
            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feedback_customer_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
