using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Saas_Dormitory.DAL.Data;

public partial class DormitoryDbContext : DbContext
{
    public DormitoryDbContext()
    {
    }

    public DormitoryDbContext(DbContextOptions<DormitoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Bed> Beds { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Stay> Stays { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Subscriptionplan> Subscriptionplans { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<Userprofile> Userprofiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ballast.proxy.rlwy.net;Port=28540;Database=postgres;Username=postgres;Password=eTDkGNMsSqmDacAyXUgoQxiUyuLJwxyZ");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.TenantId).HasDefaultValue(0);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("auditlogs_pkey");

            entity.ToTable("auditlogs");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.Action)
                .HasMaxLength(200)
                .HasColumnName("action");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Auditlogs)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("fk_auditlogs_tenant");

            entity.HasOne(d => d.User).WithMany(p => p.Auditlogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_auditlogs_user");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.BedId).HasName("beds_pkey");

            entity.ToTable("beds");

            entity.Property(e => e.BedId).HasColumnName("bed_id");
            entity.Property(e => e.BedNumber)
                .HasMaxLength(20)
                .HasColumnName("bed_number");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IsOccupied)
                .HasDefaultValue(false)
                .HasColumnName("is_occupied");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Room).WithMany(p => p.Beds)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("beds_room_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Beds)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("beds_tenant_id_fkey");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.GuestId).HasName("guests_pkey");

            entity.ToTable("guests");

            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.IdProof).HasColumnName("id_proof");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Guests)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("guests_tenant_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("invoices_pkey");

            entity.ToTable("invoices");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.InvoiceMonth).HasColumnName("invoice_month");
            entity.Property(e => e.RentAmount)
                .HasPrecision(10, 2)
                .HasColumnName("rent_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StayId).HasColumnName("stay_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Stay).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.StayId)
                .HasConstraintName("invoices_stay_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("invoices_tenant_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("fk_notifications_tenant");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_notifications_user");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .HasColumnName("reference_no");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("payments_invoice_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("payments_tenant_id_fkey");
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.PropertyId).HasName("properties_pkey");

            entity.ToTable("properties");

            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .HasColumnName("state");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Properties)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("properties_tenant_id_fkey");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("rooms_pkey");

            entity.ToTable("rooms");

            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Capacity)
                .HasDefaultValue(1)
                .HasColumnName("capacity");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PropertyId).HasColumnName("property_id");
            entity.Property(e => e.RentAmount)
                .HasPrecision(10, 2)
                .HasColumnName("rent_amount");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(20)
                .HasColumnName("room_number");
            entity.Property(e => e.RoomType)
                .HasMaxLength(50)
                .HasColumnName("room_type");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Property).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.PropertyId)
                .HasConstraintName("rooms_property_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("rooms_tenant_id_fkey");
        });

        modelBuilder.Entity<Stay>(entity =>
        {
            entity.HasKey(e => e.StayId).HasName("stays_pkey");

            entity.ToTable("stays");

            entity.Property(e => e.StayId).HasColumnName("stay_id");
            entity.Property(e => e.BedId).HasColumnName("bed_id");
            entity.Property(e => e.CheckIn)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("check_in");
            entity.Property(e => e.CheckOut)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Bed).WithMany(p => p.Stays)
                .HasForeignKey(d => d.BedId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("stays_bed_id_fkey");

            entity.HasOne(d => d.Guest).WithMany(p => p.Stays)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("stays_guest_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Stays)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("stays_tenant_id_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("subscriptions_pkey");

            entity.ToTable("subscriptions");

            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("fk_subscriptions_plan");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("fk_subscriptions_tenant");
        });

        modelBuilder.Entity<Subscriptionplan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("subscriptionplans_pkey");

            entity.ToTable("subscriptionplans");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.MaxProperties).HasColumnName("max_properties");
            entity.Property(e => e.MaxUsers).HasColumnName("max_users");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.TenantId).HasName("tenants_pkey");

            entity.ToTable("tenants");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TenantName)
                .HasMaxLength(200)
                .HasColumnName("tenant_name");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<Userprofile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("userprofiles_pkey");

            entity.ToTable("userprofiles");

            entity.HasIndex(e => e.UserId, "userprofiles_user_id_key").IsUnique();

            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Propertyid).HasColumnName("propertyid");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Userprofiles)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("fk_userprofiles_tenant");

            entity.HasOne(d => d.User).WithOne(p => p.Userprofile)
                .HasForeignKey<Userprofile>(d => d.UserId)
                .HasConstraintName("fk_userprofiles_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
