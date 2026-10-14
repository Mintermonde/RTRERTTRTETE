-- ============================================================================
-- 1. СОЗДАНИЕ ПОЛЬЗОВАТЕЛЬСКИХ ТИПОВ ДАННЫХ (ENUM)
-- ============================================================================

CREATE TYPE uav_aerodynamic_schema AS ENUM (
    'Fixed-wing',      -- Самолетный тип
    'Multirotor',      -- Мультироторный тип (коптеры)
    'VTOL',            -- Гибридный тип (вертикальный взлет-посадка + крыло)
    'Helicopter'       -- Вертолетный тип
);

CREATE TYPE uav_weight_category AS ENUM (
    'Under_150g',      -- До 150 грамм
    '150g_to_30kg',    -- От 150 г до 30 кг
    'Over_30kg'        -- Свыше 30 кг
);

CREATE TYPE uav_instance_status AS ENUM (
    'Active',          -- В эксплуатации
    'Maintenance',     -- На ТО / ремонте
    'Storage',         -- На хранении
    'Decommissioned'   -- Списан
);

CREATE TYPE maintenance_type AS ENUM (
    'Pre-flight',      -- Предполетный
    'Post-flight',     -- Послеполетный
    'TO-1',            -- ТО-1 (25-50 ч)
    'TO-2',            -- ТО-2 (100-150 ч)
    'Seasonal'         -- Сезонное ТО
);

CREATE TYPE owner_legal_type AS ENUM (
    'Individual',      -- Физическое лицо
    'IP',              -- ИП
    'LE'               -- Юридическое лицо
);

CREATE TYPE uav_control_type AS ENUM (
    'Autonomous',        -- Полностью автономный полет по миссии
    'Semi-Autonomous',   -- Полуавтономный (автопилот с вмешательством оператора)
    'Manual_Radio',      -- Ручное радиокомандное управление (RC)
    'Manual_FPV'         -- Ручной пилотаж от первого лица (FPV)
);

CREATE TYPE component_condition AS ENUM (
    'New',               -- Новая оригинальная запчасть
    'Refurbished',       -- Восстановленная деталь
    'Used'               -- Б/У (бывшая в употреблении)
);


-- ============================================================================
-- 2. СОЗДАНИЕ ТАБЛИЦ (СТРОГИЙ ПОРЯДОК: ОТ НЕЗАВИСИМЫХ К ЗАВИСИМЫМ)
-- ============================================================================

-- Таблица производителей
CREATE TABLE manufacturers (
    manufacturer_id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL UNIQUE,
    country VARCHAR(100) DEFAULT 'Россия',
    inn VARCHAR(12),
    website VARCHAR(255),
    contact_phone VARCHAR(20),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Таблица владельцев БПЛА
CREATE TABLE uav_owners (
    owner_id SERIAL PRIMARY KEY,
    legal_type owner_legal_type NOT NULL,
    name VARCHAR(255) NOT NULL,
    inn VARCHAR(12),
    ogrn VARCHAR(15),
    snils VARCHAR(11),
    contact_email VARCHAR(100),
    contact_phone VARCHAR(20),
    legal_address TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Таблица пилотов / пользователей БПЛА
CREATE TABLE uav_users (
    user_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50),
    pilot_license_number VARCHAR(50),
    license_issue_date DATE,
    license_expiry_date DATE,
    medical_certificate_number VARCHAR(50),
    phone VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100) UNIQUE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Таблица базовых моделей БПЛА
CREATE TABLE uav_models (
    model_id SERIAL PRIMARY KEY,
    manufacturer_id INT NOT NULL,
    model_name VARCHAR(100) NOT NULL,
    aerodynamic_schema uav_aerodynamic_schema NOT NULL,
    weight_category uav_weight_category NOT NULL,
    control_type uav_control_type DEFAULT 'Semi-Autonomous',
    mtow_kg NUMERIC(6, 2) NOT NULL,
    max_flight_time_mins INT,
    max_range_km NUMERIC(6, 2),
    cruising_speed_kmh INT,
    max_altitude_m INT,                      -- Практический потолок высоты (м)
    max_wind_speed_ms NUMERIC(4, 1),         -- Макс. допустимая скорость ветра (м/с)
    min_temp_celsius INT DEFAULT -20,        -- Минимальная рабочая температура (°C)
    max_temp_celsius INT DEFAULT 40,         -- Максимальная рабочая температура (°C)
    length_mm INT,                           -- Длина (мм)
    width_wingspan_mm INT,                   -- Ширина / размах крыла (мм)
    height_mm INT,                           -- Высота (мм)
    base_price_rub NUMERIC(12, 2),           -- Базовая стоимость (руб)
    CONSTRAINT fk_model_manufacturer FOREIGN KEY (manufacturer_id) 
        REFERENCES manufacturers(manufacturer_id) ON DELETE RESTRICT,
    CONSTRAINT uq_manufacturer_model UNIQUE (manufacturer_id, model_name)
);

-- Таблица допусков пилотов к моделям (Связь Многие-ко-Многим)
CREATE TABLE user_model_permissions (
    permission_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    model_id INT NOT NULL,
    granted_date DATE NOT NULL DEFAULT CURRENT_DATE,
    insurance_policy_number VARCHAR(100),
    CONSTRAINT fk_permission_user FOREIGN KEY (user_id) 
        REFERENCES uav_users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_permission_model FOREIGN KEY (model_id) 
        REFERENCES uav_models(model_id) ON DELETE CASCADE,
    CONSTRAINT uq_user_model UNIQUE (user_id, model_id)
);

-- Таблица конкретных физических бортов БПЛА
CREATE TABLE uav_instances (
    instance_id SERIAL PRIMARY KEY,
    model_id INT NOT NULL,
    owner_id INT,
    serial_number VARCHAR(100) NOT NULL UNIQUE,
    rosaviatsia_number VARCHAR(50),
    production_date DATE,
    total_flight_time_hours NUMERIC(7, 2) DEFAULT 0.00,
    current_status uav_instance_status DEFAULT 'Storage',
    hull_insurance_policy VARCHAR(100),      -- Номер полиса страхования БАС
    insurance_expiry_date DATE,              -- Срок действия полиса
    primary_color VARCHAR(50),               -- Основной цвет фюзеляжа
    last_update TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_instance_model FOREIGN KEY (model_id) 
        REFERENCES uav_models(model_id) ON DELETE RESTRICT,
    CONSTRAINT fk_uav_owner FOREIGN KEY (owner_id) 
        REFERENCES uav_owners(owner_id) ON DELETE RESTRICT
);

-- Каталог номенклатуры запчастей (BOM-справочник с Вашим ограничением CHECK)
CREATE TABLE components_catalog (
    component_type_id SERIAL PRIMARY KEY,
    manufacturer_id INT NOT NULL,
    part_number VARCHAR(100) NOT NULL UNIQUE,
    component_name VARCHAR(150) NOT NULL,
    category VARCHAR(100) NOT NULL,
    mtbf_hours INT,
    weight_g INT,
    specifications JSONB,                    -- Спецификации в JSON формате под разные ТТХ деталей
    CONSTRAINT fk_component_manufacturer FOREIGN KEY (manufacturer_id) 
        REFERENCES manufacturers(manufacturer_id) ON DELETE RESTRICT,
    CONSTRAINT chk_component_category CHECK (
        category IN (
            'Airframe', 'Engine_Electric', 'Engine_ICE', 'ESC', 
            'Autopilot', 'GNSS_RTK', 'Telemetry_Radio', 'Battery', 
            'Payload_Optics', 'Payload_LiDAR', 'Rescue_System'
        )
    )
);

-- Таблица установленных на борт деталей
CREATE TABLE instance_components (
    installed_component_id SERIAL PRIMARY KEY,
    instance_id INT NOT NULL,
    component_type_id INT NOT NULL,
    component_serial_number VARCHAR(100) NOT NULL,
    installation_date DATE NOT NULL,
    component_operating_hours NUMERIC(7, 2) DEFAULT 0.00,
    is_active BOOLEAN DEFAULT TRUE,
    condition component_condition DEFAULT 'New',            -- Состояние при установке (Новая/БУ)
    battery_charge_cycles_initial INT DEFAULT 0,            -- Исходный износ для БУ аккумуляторов
    CONSTRAINT fk_installed_uav FOREIGN KEY (instance_id) 
        REFERENCES uav_instances(instance_id) ON DELETE CASCADE,
    CONSTRAINT fk_installed_type FOREIGN KEY (component_type_id) 
        REFERENCES components_catalog(component_type_id) ON DELETE RESTRICT,
    CONSTRAINT uq_active_component_serial UNIQUE (component_serial_number)
);

-- Журнал технического обслуживания и ремонтов (ТОиР)
CREATE TABLE maintenance_logs (
    log_id SERIAL PRIMARY KEY,
    instance_id INT NOT NULL,
    type_of_maintenance maintenance_type NOT NULL,
    maintenance_date TIMESTAMP WITH TIME ZONE NOT NULL,
    uav_operating_hours_at_maintenance NUMERIC(7, 2) NOT NULL,
    description TEXT,
    engineer_name VARCHAR(150) NOT NULL,
    next_maintenance_date DATE,
    CONSTRAINT fk_maintenance_uav FOREIGN KEY (instance_id) 
        REFERENCES uav_instances(instance_id) ON DELETE CASCADE
);

-- Таблица фиксации замен компонентов во время ТО
CREATE TABLE maintenance_component_replacements (
    replacement_id SERIAL PRIMARY KEY,
    log_id INT NOT NULL,
    removed_component_id INT,
    installed_component_id INT,
    reason_for_replacement TEXT,
    CONSTRAINT fk_replacement_log FOREIGN KEY (log_id) 
        REFERENCES maintenance_logs(log_id) ON DELETE CASCADE,
    CONSTRAINT fk_replacement_removed FOREIGN KEY (removed_component_id) 
        REFERENCES instance_components(installed_component_id),
    CONSTRAINT fk_replacement_installed FOREIGN KEY (installed_component_id) 
        REFERENCES instance_components(installed_component_id)
);


-- ============================================================================
-- 3. СОЗДАНИЕ ИНДЕКСОВ ДЛЯ ОПТИМИЗАЦИИ ПРОИЗВОДИТЕЛЬНОСТИ
-- ============================================================================

CREATE INDEX idx_uav_models_manufacturer ON uav_models(manufacturer_id);
CREATE INDEX idx_uav_models_control_type ON uav_models(control_type);
CREATE INDEX idx_uav_instances_model ON uav_instances(model_id);
CREATE INDEX idx_uav_instances_owner ON uav_instances(owner_id);
CREATE INDEX idx_uav_instances_status ON uav_instances(current_status);
CREATE INDEX idx_uav_users_license ON uav_users(pilot_license_number);
CREATE INDEX idx_permissions_user ON user_model_permissions(user_id);
CREATE INDEX idx_components_catalog_category ON components_catalog(category);
CREATE INDEX idx_instance_components_uav ON instance_components(instance_id) WHERE is_active = TRUE;
CREATE INDEX idx_instance_components_condition ON instance_components(condition);
CREATE INDEX idx_maintenance_logs_uav ON maintenance_logs(instance_id);
