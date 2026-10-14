-- ============================================================================
-- 2. РАСШИРЕНИЕ БАЗЫ ДАННЫХ: ИНЖЕНЕРЫ И СИСТЕМА УЧЕТНЫХ ЗАПИСЕЙ (AUTH/IDENTITY)
-- ============================================================================

-- Справочник инженеров (специалистов по ТОиР)
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

-- Добавление внешнего ключа на инженера в журнал ТОиР
ALTER TABLE maintenance_logs 
    ADD COLUMN IF NOT EXISTS engineer_id INT,
    ADD CONSTRAINT fk_maintenance_engineer FOREIGN KEY (engineer_id) 
        REFERENCES engineers(engineer_id) ON DELETE SET NULL;

-- Учетные записи обычных пользователей (пилотов, операторов, инженеров)
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
    last_login_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT fk_user_cred_user FOREIGN KEY (linked_user_id) 
        REFERENCES uav_users(user_id) ON DELETE SET NULL,
    CONSTRAINT fk_user_cred_engineer FOREIGN KEY (linked_engineer_id) 
        REFERENCES engineers(engineer_id) ON DELETE SET NULL
);

-- Учетные записи суперпользователей (администраторов платформы)
CREATE TABLE IF NOT EXISTS superuser_credentials (
    superuser_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    security_stamp VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS idx_engineers_cert ON engineers(certification_number);
CREATE INDEX IF NOT EXISTS idx_user_credentials_username ON user_credentials(username);
CREATE INDEX IF NOT EXISTS idx_superuser_credentials_username ON superuser_credentials(username);
