using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Insurance;
using UavFleetManagement.Domain.Entities.Maintenance;
using UavFleetManagement.Domain.Entities.Organizations;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Entities.Pilots;
using UavFleetManagement.Domain.Entities.Registration;
using UavFleetManagement.Domain.Entities.Storage;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Infrastructure.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<UavOwner> UavOwners => Set<UavOwner>();
    public DbSet<UavUser> UavUsers => Set<UavUser>();
    public DbSet<UavModel> UavModels => Set<UavModel>();
    public DbSet<UserModelPermission> UserModelPermissions => Set<UserModelPermission>();
    public DbSet<UavInstance> UavInstances => Set<UavInstance>();
    public DbSet<ComponentCatalog> ComponentsCatalog => Set<ComponentCatalog>();
    public DbSet<InstanceComponent> InstanceComponents => Set<InstanceComponent>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<MaintenanceComponentReplacement> MaintenanceComponentReplacements => Set<MaintenanceComponentReplacement>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
    public DbSet<SuperuserCredential> SuperuserCredentials => Set<SuperuserCredential>();

    // New EDA & Enterprise Expansions
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<MinimumWageRate> MinimumWageRates => Set<MinimumWageRate>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<UavRegistrationApplication> UavRegistrationApplications => Set<UavRegistrationApplication>();
    public DbSet<InsurancePolicy> InsurancePolicies => Set<InsurancePolicy>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();
    public DbSet<EngineerRepairAuthorization> EngineerRepairAuthorizations => Set<EngineerRepairAuthorization>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    public DbSet<InboxEvent> InboxEvents => Set<InboxEvent>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Регистрация кастомных перечислений PostgreSQL
        modelBuilder.HasPostgresEnum<UavAerodynamicSchema>("uav_aerodynamic_schema");
        modelBuilder.HasPostgresEnum<UavWeightCategory>("uav_weight_category");
        modelBuilder.HasPostgresEnum<UavInstanceStatus>("uav_instance_status");
        modelBuilder.HasPostgresEnum<MaintenanceType>("maintenance_type");
        modelBuilder.HasPostgresEnum<OwnerLegalType>("owner_legal_type");
        modelBuilder.HasPostgresEnum<UavControlType>("uav_control_type");
        modelBuilder.HasPostgresEnum<ComponentCondition>("component_condition");

        modelBuilder.HasPostgresEnum<UavApplicationType>("uav_application_type");
        modelBuilder.HasPostgresEnum<UavApplicationStatus>("uav_application_status");
        modelBuilder.HasPostgresEnum<InsurancePolicyType>("insurance_policy_type");
        modelBuilder.HasPostgresEnum<PolicyStatus>("policy_status");

        // Manufacturers
        modelBuilder.Entity<Manufacturer>(b =>
        {
            b.ToTable("manufacturers");
            b.HasKey(x => x.ManufacturerId);
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Country).HasColumnName("country").HasMaxLength(100).HasDefaultValue("Россия");
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12);
            b.Property(x => x.Website).HasColumnName("website").HasMaxLength(255);
            b.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // UavOwners
        modelBuilder.Entity<UavOwner>(b =>
        {
            b.ToTable("uav_owners");
            b.HasKey(x => x.OwnerId);
            b.Property(x => x.OwnerId).HasColumnName("owner_id");
            b.Property(x => x.LegalType).HasColumnName("legal_type");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12);
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.Snils).HasColumnName("snils").HasMaxLength(11);
            b.Property(x => x.ContactEmail).HasColumnName("contact_email").HasMaxLength(100);
            b.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            b.Property(x => x.LegalAddress).HasColumnName("legal_address");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // UavUsers
        modelBuilder.Entity<UavUser>(b =>
        {
            b.ToTable("uav_users");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
            b.Property(x => x.PilotLicenseNumber).HasColumnName("pilot_license_number").HasMaxLength(50);
            b.Property(x => x.LicenseIssueDate).HasColumnName("license_issue_date");
            b.Property(x => x.LicenseExpiryDate).HasColumnName("license_expiry_date");
            b.Property(x => x.MedicalCertificateNumber).HasColumnName("medical_certificate_number").HasMaxLength(50);
            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            b.HasIndex(x => x.Phone).IsUnique();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        });

        // UavModels
        modelBuilder.Entity<UavModel>(b =>
        {
            b.ToTable("uav_models");
            b.HasKey(x => x.ModelId);
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.ModelName).HasColumnName("model_name").HasMaxLength(100).IsRequired();
            b.Property(x => x.AerodynamicSchema).HasColumnName("aerodynamic_schema");
            b.Property(x => x.WeightCategory).HasColumnName("weight_category");
            b.Property(x => x.ControlType).HasColumnName("control_type");
            b.Property(x => x.MtowKg).HasColumnName("mtow_kg").HasPrecision(6, 2).IsRequired();
            b.Property(x => x.MaxFlightTimeMins).HasColumnName("max_flight_time_mins");
            b.Property(x => x.MaxRangeKm).HasColumnName("max_range_km").HasPrecision(6, 2);
            b.Property(x => x.CruisingSpeedKmh).HasColumnName("cruising_speed_kmh");
            b.Property(x => x.MaxAltitudeM).HasColumnName("max_altitude_m");
            b.Property(x => x.MaxWindSpeedMs).HasColumnName("max_wind_speed_ms").HasPrecision(4, 1);
            b.Property(x => x.MinTempCelsius).HasColumnName("min_temp_celsius").HasDefaultValue(-20);
            b.Property(x => x.MaxTempCelsius).HasColumnName("max_temp_celsius").HasDefaultValue(40);
            b.Property(x => x.LengthMm).HasColumnName("length_mm");
            b.Property(x => x.WidthWingspanMm).HasColumnName("width_wingspan_mm");
            b.Property(x => x.HeightMm).HasColumnName("height_mm");
            b.Property(x => x.BasePriceRub).HasColumnName("base_price_rub").HasPrecision(12, 2);

            b.HasIndex(x => new { x.ManufacturerId, x.ModelName }).IsUnique();

            b.HasOne(x => x.Manufacturer)
                .WithMany(m => m.Models)
                .HasForeignKey(x => x.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserModelPermissions
        modelBuilder.Entity<UserModelPermission>(b =>
        {
            b.ToTable("user_model_permissions");
            b.HasKey(x => x.PermissionId);
            b.Property(x => x.PermissionId).HasColumnName("permission_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.GrantedDate).HasColumnName("granted_date");
            b.Property(x => x.InsurancePolicyNumber).HasColumnName("insurance_policy_number").HasMaxLength(100);

            b.HasIndex(x => new { x.UserId, x.ModelId }).IsUnique();

            b.HasOne(x => x.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Model)
                .WithMany(m => m.Permissions)
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UavInstances
        modelBuilder.Entity<UavInstance>(b =>
        {
            b.ToTable("uav_instances");
            b.HasKey(x => x.InstanceId);
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.ModelId).HasColumnName("model_id");
            b.Property(x => x.OwnerId).HasColumnName("owner_id");
            b.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.SerialNumber).IsUnique();
            b.Property(x => x.RosaviatsiaNumber).HasColumnName("rosaviatsia_number").HasMaxLength(50);
            b.Property(x => x.ProductionDate).HasColumnName("production_date");
            b.Property(x => x.TotalFlightTimeHours).HasColumnName("total_flight_time_hours").HasPrecision(7, 2).HasDefaultValue(0.00m);
            b.Property(x => x.CurrentStatus).HasColumnName("current_status");
            b.Property(x => x.HullInsurancePolicy).HasColumnName("hull_insurance_policy").HasMaxLength(100);
            b.Property(x => x.InsuranceExpiryDate).HasColumnName("insurance_expiry_date");
            b.Property(x => x.PrimaryColor).HasColumnName("primary_color").HasMaxLength(50);
            b.Property(x => x.LastUpdate).HasColumnName("last_update");

            b.HasOne(x => x.Model)
                .WithMany(m => m.Instances)
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Owner)
                .WithMany(o => o.Instances)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ComponentsCatalog
        modelBuilder.Entity<ComponentCatalog>(b =>
        {
            b.ToTable("components_catalog");
            b.HasKey(x => x.ComponentTypeId);
            b.Property(x => x.ComponentTypeId).HasColumnName("component_type_id");
            b.Property(x => x.ManufacturerId).HasColumnName("manufacturer_id");
            b.Property(x => x.PartNumber).HasColumnName("part_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.PartNumber).IsUnique();
            b.Property(x => x.ComponentName).HasColumnName("component_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            b.Property(x => x.MtbfHours).HasColumnName("mtbf_hours");
            b.Property(x => x.WeightG).HasColumnName("weight_g");
            b.Property(x => x.SpecificationsJson).HasColumnName("specifications").HasColumnType("jsonb");

            b.HasOne(x => x.Manufacturer)
                .WithMany(m => m.Components)
                .HasForeignKey(x => x.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InstanceComponents
        modelBuilder.Entity<InstanceComponent>(b =>
        {
            b.ToTable("instance_components");
            b.HasKey(x => x.InstalledComponentId);
            b.Property(x => x.InstalledComponentId).HasColumnName("installed_component_id");
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.ComponentTypeId).HasColumnName("component_type_id");
            b.Property(x => x.ComponentSerialNumber).HasColumnName("component_serial_number").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.ComponentSerialNumber).IsUnique();
            b.Property(x => x.InstallationDate).HasColumnName("installation_date").IsRequired();
            b.Property(x => x.ComponentOperatingHours).HasColumnName("component_operating_hours").HasPrecision(7, 2).HasDefaultValue(0.00m);
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            b.Property(x => x.Condition).HasColumnName("condition");
            b.Property(x => x.BatteryChargeCyclesInitial).HasColumnName("battery_charge_cycles_initial").HasDefaultValue(0);

            b.HasOne(x => x.Instance)
                .WithMany(i => i.InstalledComponents)
                .HasForeignKey(x => x.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ComponentType)
                .WithMany(c => c.InstanceComponents)
                .HasForeignKey(x => x.ComponentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MaintenanceLogs
        modelBuilder.Entity<MaintenanceLog>(b =>
        {
            b.ToTable("maintenance_logs");
            b.HasKey(x => x.LogId);
            b.Property(x => x.LogId).HasColumnName("log_id");
            b.Property(x => x.InstanceId).HasColumnName("instance_id");
            b.Property(x => x.TypeOfMaintenance).HasColumnName("type_of_maintenance");
            b.Property(x => x.MaintenanceDate).HasColumnName("maintenance_date").IsRequired();
            b.Property(x => x.UavOperatingHoursAtMaintenance).HasColumnName("uav_operating_hours_at_maintenance").HasPrecision(7, 2).IsRequired();
            b.Property(x => x.Description).HasColumnName("description");
            b.Property(x => x.EngineerNameFallback).HasColumnName("engineer_name").HasMaxLength(150);
            b.Property(x => x.EngineerId).HasColumnName("engineer_id");
            b.Property(x => x.NextMaintenanceDate).HasColumnName("next_maintenance_date");

            b.HasOne(x => x.Instance)
                .WithMany(i => i.MaintenanceLogs)
                .HasForeignKey(x => x.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Engineer)
                .WithMany(e => e.MaintenanceLogs)
                .HasForeignKey(x => x.EngineerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MaintenanceComponentReplacements
        modelBuilder.Entity<MaintenanceComponentReplacement>(b =>
        {
            b.ToTable("maintenance_component_replacements");
            b.HasKey(x => x.ReplacementId);
            b.Property(x => x.ReplacementId).HasColumnName("replacement_id");
            b.Property(x => x.LogId).HasColumnName("log_id");
            b.Property(x => x.RemovedComponentId).HasColumnName("removed_component_id");
            b.Property(x => x.InstalledComponentId).HasColumnName("installed_component_id");
            b.Property(x => x.ReasonForReplacement).HasColumnName("reason_for_replacement");

            b.HasOne(x => x.MaintenanceLog)
                .WithMany(m => m.Replacements)
                .HasForeignKey(x => x.LogId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.RemovedComponent)
                .WithMany()
                .HasForeignKey(x => x.RemovedComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.InstalledComponent)
                .WithMany()
                .HasForeignKey(x => x.InstalledComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Engineers
        modelBuilder.Entity<Engineer>(b =>
        {
            b.ToTable("engineers");
            b.HasKey(x => x.EngineerId);
            b.Property(x => x.EngineerId).HasColumnName("engineer_id");
            b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            b.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
            b.Property(x => x.QualificationCategory).HasColumnName("qualification_category").HasMaxLength(100).IsRequired();
            b.Property(x => x.CertificationNumber).HasColumnName("certification_number").HasMaxLength(100).IsRequired();
            b.Property(x => x.CertificationExpiryDate).HasColumnName("certification_expiry_date").IsRequired();
            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        });

        // UserCredentials
        modelBuilder.Entity<UserCredential>(b =>
        {
            b.ToTable("user_credentials");
            b.HasKey(x => x.UserCredentialId);
            b.Property(x => x.UserCredentialId).HasColumnName("user_credential_id");
            b.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            b.Property(x => x.PasswordSalt).HasColumnName("password_salt").IsRequired();
            b.Property(x => x.LinkedUserId).HasColumnName("linked_user_id");
            b.Property(x => x.LinkedEngineerId).HasColumnName("linked_engineer_id");
            b.Property(x => x.IsBlocked).HasColumnName("is_blocked").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");

            b.HasOne(x => x.LinkedUser)
                .WithOne(u => u.UserCredential)
                .HasForeignKey<UserCredential>(x => x.LinkedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.LinkedEngineer)
                .WithOne(e => e.UserCredential)
                .HasForeignKey<UserCredential>(x => x.LinkedEngineerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SuperuserCredentials
        modelBuilder.Entity<SuperuserCredential>(b =>
        {
            b.ToTable("superuser_credentials");
            b.HasKey(x => x.SuperuserId);
            b.Property(x => x.SuperuserId).HasColumnName("superuser_id");
            b.Property(x => x.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            b.Property(x => x.PasswordSalt).HasColumnName("password_salt").IsRequired();
            b.Property(x => x.SecurityStamp).HasColumnName("security_stamp").HasMaxLength(100).IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
        });

        // 1. MinimumWageRates
        modelBuilder.Entity<MinimumWageRate>(b =>
        {
            b.ToTable("minimum_wage_rates");
            b.HasKey(x => x.RateId);
            b.Property(x => x.RateId).HasColumnName("rate_id");
            b.Property(x => x.RegionCode).HasColumnName("region_code").HasMaxLength(10).IsRequired();
            b.Property(x => x.RegionName).HasColumnName("region_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.MrotValueRub).HasColumnName("mrot_value_rub").HasPrecision(12, 2).IsRequired();
            b.Property(x => x.EffectiveFrom).HasColumnName("effective_from").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.HasIndex(x => new { x.RegionCode, x.EffectiveFrom }).IsUnique();
        });

        // 2. Organizations
        modelBuilder.Entity<Organization>(b =>
        {
            b.ToTable("organizations");
            b.HasKey(x => x.OrganizationId);
            b.Property(x => x.OrganizationId).HasColumnName("organization_id");
            b.Property(x => x.LegalName).HasColumnName("legal_name").HasMaxLength(255).IsRequired();
            b.Property(x => x.ShortName).HasColumnName("short_name").HasMaxLength(150);
            b.Property(x => x.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
            b.HasIndex(x => x.Inn).IsUnique();
            b.Property(x => x.Kpp).HasColumnName("kpp").HasMaxLength(9);
            b.Property(x => x.Ogrn).HasColumnName("ogrn").HasMaxLength(15);
            b.Property(x => x.LegalType).HasColumnName("legal_type");
            b.Property(x => x.Industry).HasColumnName("industry").HasMaxLength(100).IsRequired();
            b.Property(x => x.LegalAddress).HasColumnName("legal_address");
            b.Property(x => x.ActualAddress).HasColumnName("actual_address");
            b.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            b.Property(x => x.ContactEmail).HasColumnName("contact_email").HasMaxLength(100);
            b.Property(x => x.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
            b.Property(x => x.VerificationDate).HasColumnName("verification_date");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // 3. FileAttachments
        modelBuilder.Entity<FileAttachment>(b =>
        {
            b.ToTable("file_attachments");
            b.HasKey(x => x.FileId);
            b.Property(x => x.FileId).HasColumnName("file_id");
            b.Property(x => x.BucketName).HasColumnName("bucket_name").HasMaxLength(100).IsRequired();
            b.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(500).IsRequired();
            b.Property(x => x.OriginalFileName).HasColumnName("original_file_name").HasMaxLength(255).IsRequired();
            b.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
            b.Property(x => x.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired();
            b.Property(x => x.Sha256Checksum).HasColumnName("sha256_checksum").HasMaxLength(64).IsRequired();
            b.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(100).IsRequired();
            b.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id");
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            b.Property(x => x.StorageUrl).HasColumnName("storage_url");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // 4. UavRegistrationApplications
        modelBuilder.Entity<UavRegistrationApplication>(b =>
        {
            b.ToTable("uav_registration_applications");
            b.HasKey(x => x.ApplicationId);
            b.Property(x => x.ApplicationId).HasColumnName("application_id");
            b.Property(x => x.ApplicationNumber).HasColumnName("application_number").HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.ApplicationNumber).IsUnique();
            b.Property(x => x.ApplicationType).HasColumnName("application_type");
            b.Property(x => x.UavInstanceId).HasColumnName("uav_instance_id");
            b.Property(x => x.ApplicantOwnerId).HasColumnName("applicant_owner_id");
            b.Property(x => x.ApplicantType).HasColumnName("applicant_type");
            b.Property(x => x.UavModelName).HasColumnName("uav_model_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100).IsRequired();
            b.Property(x => x.EngineType).HasColumnName("engine_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.EngineCount).HasColumnName("engine_count").HasDefaultValue(1);
            b.Property(x => x.MtowKg).HasColumnName("mtow_kg").HasPrecision(6, 2).IsRequired();
            b.Property(x => x.MaxTakeoffWeightCategory).HasColumnName("max_takeoff_weight_category");
            b.Property(x => x.SlaWorkingDays).HasColumnName("sla_working_days").HasDefaultValue(5);
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.RejectionReason).HasColumnName("rejection_reason");
            b.Property(x => x.RosaviatsiaNumber).HasColumnName("rosaviatsia_number").HasMaxLength(50);
            b.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            b.Property(x => x.ProcessedAt).HasColumnName("processed_at");
            b.Property(x => x.SlaDeadline).HasColumnName("sla_deadline");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

            b.HasOne(x => x.ApplicantOwner)
                .WithMany()
                .HasForeignKey(x => x.ApplicantOwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.UavInstance)
                .WithMany()
                .HasForeignKey(x => x.UavInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 5. InsurancePolicies
        modelBuilder.Entity<InsurancePolicy>(b =>
        {
            b.ToTable("insurance_policies");
            b.HasKey(x => x.PolicyId);
            b.Property(x => x.PolicyId).HasColumnName("policy_id");
            b.Property(x => x.PolicyNumber).HasColumnName("policy_number").HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.PolicyNumber).IsUnique();
            b.Property(x => x.UavInstanceId).HasColumnName("uav_instance_id");
            b.Property(x => x.OwnerId).HasColumnName("owner_id");
            b.Property(x => x.PolicyType).HasColumnName("policy_type");
            b.Property(x => x.CalculatedMinSumRub).HasColumnName("calculated_min_sum_rub").HasPrecision(14, 2).IsRequired();
            b.Property(x => x.SumInsuredRub).HasColumnName("sum_insured_rub").HasPrecision(14, 2).IsRequired();
            b.Property(x => x.PremiumRub).HasColumnName("premium_rub").HasPrecision(12, 2).IsRequired();
            b.Property(x => x.CommissionPercent).HasColumnName("commission_percent").HasPrecision(5, 2).HasDefaultValue(30.00m);
            b.Property(x => x.CommissionRub).HasColumnName("commission_rub").HasPrecision(12, 2).IsRequired();
            b.Property(x => x.InsuranceCompanyName).HasColumnName("insurance_company_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
            b.Property(x => x.ExpiryDate).HasColumnName("expiry_date").IsRequired();
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            b.HasOne(x => x.UavInstance)
                .WithMany()
                .HasForeignKey(x => x.UavInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 6. BillingInvoices
        modelBuilder.Entity<BillingInvoice>(b =>
        {
            b.ToTable("billing_invoices");
            b.HasKey(x => x.InvoiceId);
            b.Property(x => x.InvoiceId).HasColumnName("invoice_id");
            b.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.InvoiceNumber).IsUnique();
            b.Property(x => x.OrderId).HasColumnName("order_id").HasMaxLength(100).IsRequired();
            b.Property(x => x.ServiceType).HasColumnName("service_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.PayerOwnerId).HasColumnName("payer_owner_id");
            b.Property(x => x.AmountRub).HasColumnName("amount_rub").HasPrecision(12, 2).IsRequired();
            b.Property(x => x.PaymentStatus).HasColumnName("payment_status").HasMaxLength(30).IsRequired();
            b.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            b.Property(x => x.PaymentReference).HasColumnName("payment_reference").HasMaxLength(100);
            b.Property(x => x.PaidAt).HasColumnName("paid_at");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            b.HasOne(x => x.PayerOwner)
                .WithMany()
                .HasForeignKey(x => x.PayerOwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 7. EngineerRepairAuthorizations
        modelBuilder.Entity<EngineerRepairAuthorization>(b =>
        {
            b.ToTable("engineer_repair_authorizations");
            b.HasKey(x => x.AuthId);
            b.Property(x => x.AuthId).HasColumnName("auth_id");
            b.Property(x => x.EngineerId).HasColumnName("engineer_id");
            b.Property(x => x.AllowedUavModelId).HasColumnName("allowed_uav_model_id");
            b.Property(x => x.AllowedEngineType).HasColumnName("allowed_engine_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.MaintenanceCategories).HasColumnName("maintenance_categories").HasMaxLength(100).IsRequired();
            b.Property(x => x.CertificateEndorsementCode).HasColumnName("certificate_endorsement_code").HasMaxLength(100).IsRequired();
            b.Property(x => x.ValidUntil).HasColumnName("valid_until").IsRequired();
            b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Engineer)
                .WithMany()
                .HasForeignKey(x => x.EngineerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.AllowedUavModel)
                .WithMany()
                .HasForeignKey(x => x.AllowedUavModelId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 8. OutboxEvents
        modelBuilder.Entity<OutboxEvent>(b =>
        {
            b.ToTable("outbox_events");
            b.HasKey(x => x.EventId);
            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.AggregateType).HasColumnName("aggregate_type").HasMaxLength(50).IsRequired();
            b.Property(x => x.AggregateId).HasColumnName("aggregate_id").HasMaxLength(100).IsRequired();
            b.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            b.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            b.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            b.Property(x => x.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            b.Property(x => x.LastError).HasColumnName("last_error");
            b.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        });

        // 9. InboxEvents
        modelBuilder.Entity<InboxEvent>(b =>
        {
            b.ToTable("inbox_events");
            b.HasKey(x => new { x.EventId, x.HandlerName });
            b.Property(x => x.EventId).HasColumnName("event_id");
            b.Property(x => x.HandlerName).HasColumnName("handler_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        });

        // 10. UserRefreshTokens
        modelBuilder.Entity<UserRefreshToken>(b =>
        {
            b.ToTable("user_refresh_tokens");
            b.HasKey(x => x.RefreshTokenId);
            b.Property(x => x.RefreshTokenId).HasColumnName("refresh_token_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.SuperuserId).HasColumnName("superuser_id");
            b.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
            b.Property(x => x.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(50);
            b.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            b.Property(x => x.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash").HasMaxLength(128);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Superuser)
                .WithMany()
                .HasForeignKey(x => x.SuperuserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
