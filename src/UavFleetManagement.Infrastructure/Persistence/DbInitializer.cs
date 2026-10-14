using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Insurance;
using UavFleetManagement.Domain.Entities.Organizations;
using UavFleetManagement.Domain.Entities.Registration;
using UavFleetManagement.Domain.Enums;
using UavFleetManagement.Infrastructure.Persistence.Contexts;

namespace UavFleetManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher hasher)
    {
        // 1. Создаем необходимые таблицы расширения (если контейнер был запущен со старым томом)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS engineers (
    engineer_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50),
    qualification_category VARCHAR(100) NOT NULL,
    certification_number VARCHAR(100) NOT NULL,
    certification_expiry_date DATE NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE
);

DO $$ 
BEGIN 
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name='maintenance_logs') THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='maintenance_logs' AND column_name='engineer_id') THEN
            ALTER TABLE maintenance_logs ADD COLUMN engineer_id INT REFERENCES engineers(engineer_id) ON DELETE SET NULL;
        END IF;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS user_credentials (
    user_credential_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    linked_user_id INT,
    linked_engineer_id INT,
    is_blocked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS superuser_credentials (
    superuser_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    security_stamp VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE
);

-- Таблицы EDA и расширения
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uav_application_type') THEN
        CREATE TYPE uav_application_type AS ENUM (
            'InitialRegistration', 'ModificationOfData', 'Deregistration',
            'DuplicateOrExtract', 'SlgCertification', 'TypeCertification', 'AirspaceAuthorization'
        );
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uav_application_status') THEN
        CREATE TYPE uav_application_status AS ENUM (
            'Draft', 'Pending', 'UnderReview', 'Approved', 'Rejected', 'Cancelled'
        );
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'insurance_policy_type') THEN
        CREATE TYPE insurance_policy_type AS ENUM (
            'CivilLiabilityVzk131', 'HullCasco', 'PayloadEquipment', 'ComprehensivePackage'
        );
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'policy_status') THEN
        CREATE TYPE policy_status AS ENUM (
            'Draft', 'Pending', 'Approved', 'Active', 'Expired', 'Terminated'
        );
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS minimum_wage_rates (
    rate_id SERIAL PRIMARY KEY,
    region_code VARCHAR(10) NOT NULL DEFAULT 'FED',
    region_name VARCHAR(150) NOT NULL DEFAULT 'Российская Федерация',
    mrot_value_rub NUMERIC(12, 2) NOT NULL,
    effective_from DATE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS organizations (
    organization_id SERIAL PRIMARY KEY,
    legal_name VARCHAR(255) NOT NULL,
    short_name VARCHAR(150),
    inn VARCHAR(12) NOT NULL UNIQUE,
    kpp VARCHAR(9),
    ogrn VARCHAR(15),
    legal_type owner_legal_type NOT NULL DEFAULT 'LE',
    industry VARCHAR(100) NOT NULL,
    legal_address TEXT,
    actual_address TEXT,
    contact_phone VARCHAR(20),
    contact_email VARCHAR(100),
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    verification_date TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS file_attachments (
    file_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bucket_name VARCHAR(100) NOT NULL DEFAULT 'uav-documents',
    storage_key VARCHAR(500) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    sha256_checksum VARCHAR(64) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id VARCHAR(100) NOT NULL,
    uploaded_by_user_id INT,
    status VARCHAR(30) NOT NULL DEFAULT 'PendingVerification',
    storage_url TEXT,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS uav_registration_applications (
    application_id SERIAL PRIMARY KEY,
    application_number VARCHAR(50) NOT NULL UNIQUE,
    application_type uav_application_type NOT NULL DEFAULT 'InitialRegistration',
    uav_instance_id INT,
    applicant_owner_id INT NOT NULL,
    applicant_type owner_legal_type NOT NULL DEFAULT 'LE',
    uav_model_name VARCHAR(150) NOT NULL,
    serial_number VARCHAR(100) NOT NULL,
    engine_type VARCHAR(50) NOT NULL,
    engine_count INT NOT NULL DEFAULT 1,
    mtow_kg NUMERIC(6, 2) NOT NULL,
    max_takeoff_weight_category uav_weight_category NOT NULL,
    sla_working_days INT NOT NULL DEFAULT 5,
    status uav_application_status NOT NULL DEFAULT 'Pending',
    rejection_reason TEXT,
    rosaviatsia_number VARCHAR(50),
    submitted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMPTZ,
    sla_deadline DATE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS insurance_policies (
    policy_id SERIAL PRIMARY KEY,
    policy_number VARCHAR(50) NOT NULL UNIQUE,
    uav_instance_id INT NOT NULL,
    owner_id INT NOT NULL,
    policy_type insurance_policy_type NOT NULL DEFAULT 'CivilLiabilityVzk131',
    calculated_min_sum_rub NUMERIC(14, 2) NOT NULL,
    sum_insured_rub NUMERIC(14, 2) NOT NULL,
    premium_rub NUMERIC(12, 2) NOT NULL,
    commission_percent NUMERIC(5, 2) NOT NULL DEFAULT 30.00,
    commission_rub NUMERIC(12, 2) NOT NULL,
    insurance_company_name VARCHAR(150) NOT NULL DEFAULT 'АО ""АвиаСтрахование""',
    status policy_status NOT NULL DEFAULT 'Draft',
    start_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS billing_invoices (
    invoice_id SERIAL PRIMARY KEY,
    invoice_number VARCHAR(50) NOT NULL UNIQUE,
    order_id VARCHAR(100) NOT NULL,
    service_type VARCHAR(50) NOT NULL,
    payer_owner_id INT NOT NULL,
    amount_rub NUMERIC(12, 2) NOT NULL,
    payment_status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    payment_method VARCHAR(50),
    payment_reference VARCHAR(100),
    paid_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS engineer_repair_authorizations (
    auth_id SERIAL PRIMARY KEY,
    engineer_id INT NOT NULL,
    allowed_uav_model_id INT,
    allowed_engine_type VARCHAR(50) NOT NULL DEFAULT 'All',
    maintenance_categories VARCHAR(100) NOT NULL,
    certificate_endorsement_code VARCHAR(100) NOT NULL,
    valid_until DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS outbox_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_type VARCHAR(50) NOT NULL,
    aggregate_id VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    metadata JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    retry_count INT NOT NULL DEFAULT 0,
    last_error TEXT,
    processed_at TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS inbox_events (
    event_id UUID NOT NULL,
    handler_name VARCHAR(150) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_inbox_events PRIMARY KEY (event_id, handler_name)
);

CREATE TABLE IF NOT EXISTS user_refresh_tokens (
    refresh_token_id SERIAL PRIMARY KEY,
    user_id INT,
    superuser_id INT,
    token_hash VARCHAR(128) NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_ip VARCHAR(50),
    revoked_at TIMESTAMPTZ,
    replaced_by_token_hash VARCHAR(128)
);
");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Замечание при выполнении расширенного DDL: {ex.Message}");
        }

        // 2. Создание учетной записи Администратора (admin / Admin123!)
        try
        {
            if (!await context.SuperuserCredentials.AnyAsync(u => u.Username == "admin"))
            {
                var (adminHash, adminSalt) = hasher.HashPassword("Admin123!");
                context.SuperuserCredentials.Add(new SuperuserCredential
                {
                    Username = "admin",
                    PasswordHash = adminHash,
                    PasswordSalt = adminSalt,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Суперпользователь 'admin' успешно создан.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Ошибка создания суперпользователя: {ex.Message}");
        }

        // 3. Создание 7 ролевых пользователей для демонстрации и быстрого переключения
        var roleUsers = new (string Username, string Password, string Email)[]
        {
            ("pilot", "Pilot123!", "pilot@uav-civil.ru"),
            ("client", "Client123!", "client@uav-civil.ru"),
            ("engineer", "Engineer123!", "engineer@uav-civil.ru"),
            ("agent", "Agent123!", "agent@uav-insurance.ru"),
            ("accountant", "Accountant123!", "accountant@uav-finance.ru"),
            ("auditor", "Auditor123!", "auditor@rosaviatsia.gov.ru")
        };

        foreach (var (username, password, email) in roleUsers)
        {
            try
            {
                if (!await context.UserCredentials.AnyAsync(u => u.Username == username))
                {
                    var (uHash, uSalt) = hasher.HashPassword(password);
                    context.UserCredentials.Add(new UserCredential
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = uHash,
                        PasswordSalt = uSalt,
                        CreatedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer] Ошибка создания пользователя {username}: {ex.Message}");
            }
        }

        // 4. Сидирование ставок МРОТ (ст. 131 ВЗК РФ)
        try
        {
            if (!await context.MinimumWageRates.AnyAsync())
            {
                context.MinimumWageRates.AddRange(
                    new MinimumWageRate
                    {
                        RegionCode = "FED",
                        RegionName = "Российская Федерация (базовая ставка)",
                        MrotValueRub = 19242.00m,
                        EffectiveFrom = new DateOnly(2024, 1, 1),
                        CreatedAt = DateTime.UtcNow
                    },
                    new MinimumWageRate
                    {
                        RegionCode = "FED_2026",
                        RegionName = "Российская Федерация (плановая ставка)",
                        MrotValueRub = 22440.00m,
                        EffectiveFrom = new DateOnly(2026, 1, 1),
                        CreatedAt = DateTime.UtcNow
                    },
                    new MinimumWageRate
                    {
                        RegionCode = "77",
                        RegionName = "г. Москва",
                        MrotValueRub = 29389.00m,
                        EffectiveFrom = new DateOnly(2024, 1, 1),
                        CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Замечание при сидировании МРОТ: {ex.Message}");
        }

        // 5. Сидирование открытого реестра компаний и эксплуатантов
        try
        {
            if (!await context.Organizations.AnyAsync())
            {
                context.Organizations.AddRange(
                    new Organization
                    {
                        LegalName = "ООО \"Геоскан Агро\"",
                        ShortName = "Геоскан Агро",
                        Inn = "7814472390",
                        Kpp = "781401001",
                        Ogrn = "1157847001234",
                        LegalType = OwnerLegalType.LE,
                        Industry = "АПК (Сельское хозяйство)",
                        LegalAddress = "г. Санкт-Петербург, ул. Шаврова, д. 12",
                        ActualAddress = "г. Санкт-Петербург, ул. Шаврова, д. 12",
                        ContactPhone = "+7 (812) 363-33-33",
                        ContactEmail = "agro@geoscan.aero",
                        IsVerified = true,
                        VerificationDate = DateTime.UtcNow.AddMonths(-3)
                    },
                    new Organization
                    {
                        LegalName = "АО \"Беспилотные Технологии Мониторинга\"",
                        ShortName = "БТМ",
                        Inn = "7701234567",
                        Kpp = "770101001",
                        Ogrn = "1187746005678",
                        LegalType = OwnerLegalType.LE,
                        Industry = "ТЭК (Нефтегазовый сектор)",
                        LegalAddress = "г. Москва, Пресненская наб., д. 8",
                        ActualAddress = "г. Москва, Пресненская наб., д. 8",
                        ContactPhone = "+7 (495) 789-01-23",
                        ContactEmail = "info@btm-uav.ru",
                        IsVerified = true,
                        VerificationDate = DateTime.UtcNow.AddMonths(-1)
                    },
                    new Organization
                    {
                        LegalName = "ИП Соколов Роман Викторович",
                        ShortName = "ИП Соколов Р.В.",
                        Inn = "500109876543",
                        Ogrn = "319500100012345",
                        LegalType = OwnerLegalType.IP,
                        Industry = "Геодезия и картография",
                        LegalAddress = "Московская обл., г. Балашиха, ул. Ленина, д. 5",
                        ActualAddress = "Московская обл., г. Балашиха, ул. Ленина, д. 5",
                        ContactPhone = "+7 (916) 555-44-33",
                        ContactEmail = "sokolov.geo@mail.ru",
                        IsVerified = true,
                        VerificationDate = DateTime.UtcNow.AddDays(-10)
                    }
                );
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Замечание при сидировании организаций: {ex.Message}");
        }

        // 6. Тестовый производитель, модель, инженер, борт
        try
        {
            if (!await context.Manufacturers.AnyAsync())
            {
                var manufacturer = new Manufacturer
                {
                    Name = "ООО \"Геоскан\"",
                    Country = "Россия",
                    Inn = "7814472390",
                    Website = "https://geoscan.aero",
                    ContactPhone = "+7 (812) 363-33-33",
                    CreatedAt = DateTime.UtcNow
                };
                context.Manufacturers.Add(manufacturer);
                await context.SaveChangesAsync();

                var model = new UavModel
                {
                    ManufacturerId = manufacturer.ManufacturerId,
                    ModelName = "Геоскан 401",
                    AerodynamicSchema = UavAerodynamicSchema.Multirotor,
                    WeightCategory = UavWeightCategory.Range150gTo30kg,
                    ControlType = UavControlType.SemiAutonomous,
                    MtowKg = 9.30m,
                    MaxFlightTimeMins = 60,
                    MaxRangeKm = 24.00m,
                    CruisingSpeedKmh = 40,
                    MaxAltitudeM = 1000,
                    MaxWindSpeedMs = 12.0m,
                    MinTempCelsius = -20,
                    MaxTempCelsius = 40,
                    LengthMm = 1250,
                    WidthWingspanMm = 1250,
                    HeightMm = 550,
                    BasePriceRub = 1850000.00m
                };
                context.UavModels.Add(model);

                var engineer = new Engineer
                {
                    FirstName = "Алексей",
                    LastName = "Смирнов",
                    MiddleName = "Игоревич",
                    QualificationCategory = "Инженер ТОиР 1-й категории",
                    CertificationNumber = "CERT-UAV-2024-001",
                    CertificationExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                    Phone = "+7 (999) 111-22-33",
                    Email = "smirnov.a@fleet.ru",
                    IsActive = true
                };
                context.Engineers.Add(engineer);

                var owner = new UavOwner
                {
                    LegalType = OwnerLegalType.LE,
                    Name = "ООО \"Аэромониторинг\"",
                    Inn = "7701234567",
                    Ogrn = "1157746001234",
                    ContactEmail = "info@aeromonitoring.ru",
                    ContactPhone = "+7 (495) 789-01-23",
                    LegalAddress = "г. Москва, ул. Авиаконструкторов, д. 12",
                    CreatedAt = DateTime.UtcNow
                };
                context.UavOwners.Add(owner);
                await context.SaveChangesAsync();

                var instance = new UavInstance
                {
                    ModelId = model.ModelId,
                    OwnerId = owner.OwnerId,
                    SerialNumber = "GS-401-2024-0042",
                    RosaviatsiaNumber = "RA-0142A",
                    ProductionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                    TotalFlightTimeHours = 45.50m,
                    CurrentStatus = UavInstanceStatus.Active,
                    HullInsurancePolicy = "POLIS-RESO-UAV-991823",
                    InsuranceExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
                    PrimaryColor = "Белый / Оранжевый",
                    LastUpdate = DateTime.UtcNow
                };
                context.UavInstances.Add(instance);

                // Допуск инженера к модели Геоскан 401
                context.EngineerRepairAuthorizations.Add(new EngineerRepairAuthorization
                {
                    EngineerId = engineer.EngineerId,
                    AllowedUavModelId = model.ModelId,
                    AllowedEngineType = "Electric",
                    MaintenanceCategories = "TO-1,TO-2,Seasonal,PreFlight,PostFlight",
                    CertificateEndorsementCode = "ENDORSE-GS-401-E",
                    ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                    IsActive = true
                });

                // Тестовый полис ст. 131 ВЗК РФ
                decimal minSum = 9.30m * 2m * 19242.00m; // 357,901.20
                decimal premium = 7500.00m;
                context.InsurancePolicies.Add(new InsurancePolicy
                {
                    PolicyNumber = "POLIS-VZK-2026-0042",
                    UavInstanceId = instance.InstanceId,
                    OwnerId = owner.OwnerId,
                    PolicyType = InsurancePolicyType.CivilLiabilityVzk131,
                    CalculatedMinSumRub = minSum,
                    SumInsuredRub = 500000.00m,
                    PremiumRub = premium,
                    CommissionPercent = 30.00m,
                    CommissionRub = premium * 0.30m,
                    InsuranceCompanyName = "АО \"АвиаСтрахование\"",
                    Status = PolicyStatus.Active,
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
                });

                // Тестовое заявление на госучет
                context.UavRegistrationApplications.Add(new UavRegistrationApplication
                {
                    ApplicationNumber = "REG-2026-00001",
                    ApplicationType = UavApplicationType.InitialRegistration,
                    ApplicantOwnerId = owner.OwnerId,
                    ApplicantType = OwnerLegalType.LE,
                    UavModelName = "Геоскан 401",
                    SerialNumber = "GS-401-2024-0042",
                    EngineType = "Electric",
                    EngineCount = 4,
                    MtowKg = 9.30m,
                    MaxTakeoffWeightCategory = UavWeightCategory.Range150gTo30kg,
                    SlaWorkingDays = 5,
                    Status = UavApplicationStatus.Approved,
                    RosaviatsiaNumber = "RA-0142A",
                    SubmittedAt = DateTime.UtcNow.AddDays(-6),
                    ProcessedAt = DateTime.UtcNow.AddDays(-1),
                    SlaDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                });

                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer] Стартовые справочники, допуски и борт успешно инициализированы.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] Замечание при сидировании справочников: {ex.Message}");
        }
    }
}
