-- ============================================================================
-- 3. РАСШИРЕНИЕ EDA, МЕТАДАННЫЕ S3, РЕГИСТРАЦИЯ БАС, СТРАХОВАНИЕ И БИЛЛИНГ
-- ============================================================================

-- Новые перечисления
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uav_application_type') THEN
        CREATE TYPE uav_application_type AS ENUM (
            'InitialRegistration',
            'ModificationOfData',
            'Deregistration',
            'DuplicateOrExtract',
            'SlgCertification',
            'TypeCertification',
            'AirspaceAuthorization'
        );
    END IF;
END $$;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'uav_application_status') THEN
        CREATE TYPE uav_application_status AS ENUM (
            'Draft',
            'Pending',
            'UnderReview',
            'Approved',
            'Rejected',
            'Cancelled'
        );
    END IF;
END $$;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'insurance_policy_type') THEN
        CREATE TYPE insurance_policy_type AS ENUM (
            'CivilLiabilityVzk131',
            'HullCasco',
            'PayloadEquipment',
            'ComprehensivePackage'
        );
    END IF;
END $$;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'policy_status') THEN
        CREATE TYPE policy_status AS ENUM (
            'Draft',
            'Pending',
            'Approved',
            'Active',
            'Expired',
            'Terminated'
        );
    END IF;
END $$;

-- 1. Справочник МРОТ
CREATE TABLE IF NOT EXISTS minimum_wage_rates (
    rate_id SERIAL PRIMARY KEY,
    region_code VARCHAR(10) NOT NULL DEFAULT 'FED',
    region_name VARCHAR(150) NOT NULL DEFAULT 'Российская Федерация',
    mrot_value_rub NUMERIC(12, 2) NOT NULL,
    effective_from DATE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_region_effective UNIQUE (region_code, effective_from)
);

-- 2. Реестр субъектов / организаций (ЮЛ / ИП)
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

CREATE INDEX IF NOT EXISTS idx_organizations_inn ON organizations(inn) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_organizations_industry ON organizations(industry) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_organizations_verified ON organizations(is_verified);

-- 3. S3 Метаданные файлов
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

CREATE INDEX IF NOT EXISTS idx_file_attachments_entity ON file_attachments(entity_type, entity_id) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_file_attachments_status ON file_attachments(status);

-- 4. Заявления на госучет БАС
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
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT fk_app_owner FOREIGN KEY (applicant_owner_id) 
        REFERENCES uav_owners(owner_id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_uav_app_status ON uav_registration_applications(status) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_uav_app_owner ON uav_registration_applications(applicant_owner_id);
CREATE INDEX IF NOT EXISTS idx_uav_app_serial ON uav_registration_applications(serial_number);

-- 5. Страховые полисы (ст. 131 ВЗК РФ + КАСКО)
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
    insurance_company_name VARCHAR(150) NOT NULL DEFAULT 'АО "АвиаСтрахование"',
    status policy_status NOT NULL DEFAULT 'Draft',
    start_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_policy_instance FOREIGN KEY (uav_instance_id) 
        REFERENCES uav_instances(instance_id) ON DELETE RESTRICT,
    CONSTRAINT fk_policy_owner FOREIGN KEY (owner_id) 
        REFERENCES uav_owners(owner_id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_insurance_policies_status ON insurance_policies(status) WHERE is_deleted = FALSE;
CREATE INDEX IF NOT EXISTS idx_insurance_policies_owner ON insurance_policies(owner_id);

-- 6. Инвойсы и биллинг
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
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_invoice_owner FOREIGN KEY (payer_owner_id) 
        REFERENCES uav_owners(owner_id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_billing_invoices_status ON billing_invoices(payment_status) WHERE is_deleted = FALSE;

-- 7. Матрица допусков инженеров ТОиР
CREATE TABLE IF NOT EXISTS engineer_repair_authorizations (
    auth_id SERIAL PRIMARY KEY,
    engineer_id INT NOT NULL,
    allowed_uav_model_id INT,
    allowed_engine_type VARCHAR(50) NOT NULL DEFAULT 'All',
    maintenance_categories VARCHAR(100) NOT NULL,
    certificate_endorsement_code VARCHAR(100) NOT NULL,
    valid_until DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_authorizations_engineer FOREIGN KEY (engineer_id) 
        REFERENCES engineers(engineer_id) ON DELETE CASCADE,
    CONSTRAINT fk_authorizations_model FOREIGN KEY (allowed_uav_model_id) 
        REFERENCES uav_models(model_id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_engineer_auth_eng ON engineer_repair_authorizations(engineer_id);

-- 8. Transactional Outbox
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

CREATE INDEX IF NOT EXISTS idx_outbox_pending ON outbox_events (created_at, retry_count) 
    WHERE status = 'Pending';

CREATE INDEX IF NOT EXISTS idx_outbox_processing ON outbox_events (processed_at) 
    WHERE status = 'Processing';

-- 9. Inbox Deduplication
CREATE TABLE IF NOT EXISTS inbox_events (
    event_id UUID NOT NULL,
    handler_name VARCHAR(150) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_inbox_events PRIMARY KEY (event_id, handler_name)
);

CREATE INDEX IF NOT EXISTS idx_inbox_processed_at ON inbox_events(processed_at);

-- 10. Refresh Tokens для ротации JWT каждые 30 минут
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
    replaced_by_token_hash VARCHAR(128),
    CONSTRAINT fk_refresh_user FOREIGN KEY (user_id) 
        REFERENCES user_credentials(user_credential_id) ON DELETE CASCADE,
    CONSTRAINT fk_refresh_superuser FOREIGN KEY (superuser_id) 
        REFERENCES superuser_credentials(superuser_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_refresh_token_hash ON user_refresh_tokens(token_hash) WHERE is_revoked = FALSE;
