# UAV Fleet Management System — Backend API

Полнофункциональная система учета, эксплуатации и сервисного обслуживания (ТОиР / MRO) гражданских беспилотных авиационных систем (БАС / БПЛА) с разделением ролей, защитой персональных данных (152-ФЗ) и нативной интеграцией с СУБД PostgreSQL.

---

## Оглавление
1. [Обзор проекта и решаемые задачи](#-обзор-проекта-и-решаемые-задачи)
2. [Технологический стек](#-технологический-стек)
3. [Архитектура решения (Clean Architecture)](#-архитектура-решения-clean-architecture)
4. [Структура базы данных и доменная модель](#-структура-базы-данных-и-доменная-модель)
5. [Безопасность, аутентификация и Data Privacy](#-безопасность-аутентификация-и-data-privacy)
6. [Техническая подноготная и инженерные решения](#-техническая-подноготная-и-инженерные-решения)
7. [Интерактивный клиент и Swagger UI](#-интерактивный-клиент-и-swagger-ui)
8. [Карта API (REST-контроллеры)](#-карта-api-rest-контроллеры)
9. [Инструкция по развертыванию и запуску](#-инструкция-по-развертыванию-и-запуску)
10. [Тестовые учетные записи](#-тестовые-учетные-записи)

---

## Обзор проекта и решаемые задачи

Система разработана для комплексного контроля за парком гражданских беспилотных воздушных судов в соответствии с воздушным законодательством РФ и требованиями Росавиации. 

### Основные функциональные блоки:
- **Учет парка бортов:** Регистрация физических экземпляров БАС, привязка учетных номеров Росавиации, серийных номеров, учет наработки моточасов и полисов страхования КАСКО.
- **Справочники и модели:** Каталог производителей техники, спецификации серийных линеек БПЛА с летными и весогабаритными ТТХ.
- **BOM-каталог комплектующих (Bill of Materials):** Учет установленных на борта узлов, двигателей, регуляторов, полезных нагрузок и трекинг остаточного ресурса аккумуляторов (циклы заряда).
- **Летный состав и матрица допусков:** Реестр внешних пилотов, сроки действия свидетельств, медсправок ВЛЭК и фиксация квалификационных допусков пилотов к конкретным моделям.
- **ТОиР (MRO) и инженеры:** Журнал регламентных технических обслуживаний (предполетные, послеполетные, ТО-1, ТО-2, сезонные) со связью с сертифицированными специалистами и фиксацией замены узлов.

---

## Технологический стек

- **Язык программирования:** C# 12
- **Платформа:** ASP.NET Core (.NET 8 LTS)
- **СУБД:** PostgreSQL 15 (Docker)
- **ORM:** Entity Framework Core 8 + Npgsql.EntityFrameworkCore.PostgreSQL
- **Аутентификация:** JWT Bearer (JSON Web Tokens)
- **Хэширование паролей:** PBKDF2 с HMAC-SHA512 + криптографическая соль
- **Документация:** OpenAPI / Swagger UI (Swashbuckle 6.9)
- **Фронтенд-клиент:** Single Page Application (HTML5, Bootstrap 5, Vanilla JS / Fetch API)

---

## Архитектура решения (Clean Architecture)

Проект спроектирован по принципам **Clean Architecture / N-Tier** с разделением ответственности на 4 слабосвязанных слоя внутри одного Solution (`UavFleetManagement.slnx` / `.sln`):

```text
UavFleetManagement/
│
├── 1. Core / Domain (UavFleetManagement.Domain)
│   ├── Entities/                 # 13 сущностей предметной области
│   │   ├── Catalog/              # Manufacturer, UavModel, ComponentCatalog
│   │   ├── Fleet/                # UavOwner, UavInstance, InstanceComponent
│   │   ├── Pilots/               # UavUser, UserModelPermission
│   │   ├── Maintenance/          # MaintenanceLog, MaintenanceComponentReplacement
│   │   ├── Engineers/            # Engineer
│   │   └── Identity/             # UserCredential, SuperuserCredential
│   └── Enums/                    # 7 C# Enums, маппящихся на ENUM-типы PostgreSQL
│
├── 2. Business Logic (UavFleetManagement.Application)
│   ├── Common/
│   │   ├── Interfaces/           # IApplicationDbContext, IAuthService, ITokenService, IPasswordHasher
│   │   └── Exceptions/           # NotFoundException, BadRequestException, UnauthorizedException, ForbiddenException
│   └── DTOs/                     # Контракты данных с разделением уровней приватности
│       ├── Auth/                 # LoginRequestDto, RegisterRequestDto, AuthResponseDto
│       ├── Catalog/              # ManufacturerDto, UavModelSummaryDto, UavModelDetailsDto
│       ├── Fleet/                # UavOwnerPublicDto, UavOwnerAdminDto, UavInstanceDto
│       ├── Pilots/               # UavUserPublicDto, UavUserAdminDto, UserModelPermissionDto
│       ├── Maintenance/          # MaintenanceLogDto, MaintenanceReplacementDto
│       └── Engineers/            # EngineerDto, EngineerCreateDto
│
├── 3. Infrastructure (UavFleetManagement.Infrastructure)
│   ├── Identity/                 # PasswordHasher (PBKDF2/SHA512), TokenService (JWT), AuthService
│   └── Persistence/
│       ├── Contexts/             # ApplicationDbContext (Fluent API сопоставление полей, связей, типов)
│       ├── CustomEnumNameTranslator.cs # Транслятор имен кастомных ENUM типов PostgreSQL
│       └── DbInitializer.cs      # Автоматическое DDL-создание таблиц и сидирование данных
│
└── 4. Presentation (UavFleetManagement.Api)
    ├── Controllers/              # 12 REST-контроллеров по каждой предметной таблице
    ├── Middlewares/              # ErrorHandlingMiddleware (перехват исключений и распаковка InnerException)
    ├── Properties/               # launchSettings.json (настройка портов 5000 / 7001)
    ├── wwwroot/                  # Интерактивный двухэкранный клиент index.html
    ├── appsettings.json          # Строки подключения к PostgreSQL и параметры JWT
    └── Program.cs                # Регистрация DI, конвейер HTTP, Swagger и NpgsqlDataSourceBuilder
```

### Направление зависимостей:
`Api` ──> `Application` <── `Infrastructure`
  │             │               │
  └───> `Domain` <──────────────┘

*Слой `Domain` не имеет никаких внешних зависимостей. Бизнес-правила и контракты сосредоточены в `Application`. Доступ к БД и внешние библиотеки инкапсулированы в `Infrastructure`.*

---

## Структура базы данных и доменная модель

Схема базы данных объединяет 13 таблиц и 7 пользовательских ENUM-типов данных.

### Пользовательские типы данных (PostgreSQL ENUM):
1. `uav_aerodynamic_schema`: `'Fixed-wing'`, `'Multirotor'`, `'VTOL'`, `'Helicopter'`
2. `uav_weight_category`: `'Under_150g'`, `'150g_to_30kg'`, `'Over_30kg'`
3. `uav_instance_status`: `'Active'`, `'Maintenance'`, `'Storage'`, `'Decommissioned'`
4. `maintenance_type`: `'Pre-flight'`, `'Post-flight'`, `'TO-1'`, `'TO-2'`, `'Seasonal'`
5. `owner_legal_type`: `'Individual'`, `'IP'`, `'LE'`
6. `uav_control_type`: `'Autonomous'`, `'Semi-Autonomous'`, `'Manual_Radio'`, `'Manual_FPV'`
7. `component_condition`: `'New'`, `'Refurbished'`, `'Used'`

### Сущности и таблицы:
| Таблица | Назначение | Ключевые поля и связи |
|---|---|---|
| `manufacturers` | Производители БПЛА и компонентов | `manufacturer_id`, `name` (UNIQUE), `inn`, `website` |
| `uav_models` | Серийные модели беспилотников | `model_id`, `manufacturer_id` (FK), ТТХ (MTOW, скорости, потолок), `base_price_rub` |
| `uav_owners` | Реестр владельцев (ФЛ, ИП, ЮЛ) | `owner_id`, `legal_type`, `name`, `inn`, `ogrn`, `snils`, `legal_address` |
| `uav_users` | Внешние пилоты и операторы БАС | `user_id`, ФИО, `pilot_license_number`, даты лицензии, справка ВЛЭК |
| `user_model_permissions` | Матрица допусков пилотов к моделям | `permission_id`, `user_id` (FK), `model_id` (FK), `insurance_policy_number` |
| `uav_instances` | Конкретные физические борта БПЛА | `instance_id`, `serial_number` (UNIQUE), `rosaviatsia_number`, статус, полис КАСКО |
| `components_catalog` | Каталог номенклатуры деталей (BOM) | `component_type_id`, `part_number`, `category`, `specifications` (JSONB) |
| `instance_components` | Установленные на борт детали | `installed_component_id`, `instance_id` (FK), серийный номер, наработка часов, циклы АКБ |
| `maintenance_logs` | Журнал обслуживания (ТОиР) | `log_id`, `instance_id` (FK), `engineer_id` (FK), дата, наработка при ТО, описание |
| `maintenance_component_replacements` | Фиксация замен деталей при ТО | `replacement_id`, `log_id` (FK), ссылки на снятую и установленную детали |
| `engineers` | Справочник сертифицированных инженеров | `engineer_id`, ФИО, категория допуска, номер и срок сертификата |
| `user_credentials` | Учетные записи пользователей (User) | `user_credential_id`, `username`, хэш + соль, привязка к пилоту или инженеру |
| `superuser_credentials` | Изолированные учетные записи админов | `superuser_id`, `username`, хэш + соль, `security_stamp` |

---

## Безопасность, аутентификация и Data Privacy

### 1. Двухуровневая модель доступа (RBAC):
- **Superuser (Администратор платформы):**
  - Полный CRUD-доступ ко всем каталогам, бортам, записям ТОиР и учетным записям.
  - Видит все конфиденциальные данные без цензуры (базовые цены, полисы страхования КАСКО, СНИЛС, полные лицензии пилотов).
- **User (Обычный пользователь: эксплуатант, пилот, инженер):**
  - Доступ в режиме чтения к технической информации, необходимой для выполнения полетов.
  - Персональные и коммерческие данные для роли User автоматически маскируются или скрываются.

> **Важное архитектурное решение:** Инженер (`Engineer`) в системе является **предметной справочной сущностью**, а не отдельной ролью безопасности. Аккаунт инженера авторизуется со стандартными правами уровня `User`, имея связь со своей записью в справочнике специалистов.

### 2. Динамическое маскирование данных (Data Masking):
Разделение достигается за счет специализированных DTO в слое `Application`:
- **Модели БПЛА (`uav_models`):**
  - `User` видит `UavModelSummaryDto`: базовые ТТХ без стоимости.
  - `Superuser` видит `UavModelDetailsDto`: полные ТТХ, включая поле `basePriceRub`.
- **Владельцы (`uav_owners`):**
  - `User` видит `UavOwnerPublicDto`: контакты без ПДн (поля `inn`, `ogrn`, `snils`, `legalAddress` скрыты / возвращают `null`).
  - `Superuser` видит `UavOwnerAdminDto`: полные реквизиты.
- **Пилоты (`uav_users`):**
  - `User` видит `UavUserPublicDto`: номер удостоверения маскируется алгоритмом (например, `PL-******23`), медицинская справка скрыта.
  - `Superuser` видит `UavUserAdminDto`: полные номера документов и даты их действия.
- **Борта (`uav_instances`):**
  - Для `User` поля `hullInsurancePolicy` и `insuranceExpiryDate` принудительно возвращают `null`.

### 3. Хранение учетных данных и криптография:
- Таблицы `superuser_credentials` и `user_credentials` **физически изолированы**.
- Хэширование осуществляется алгоритмом **PBKDF2 с HMAC-SHA512** (100 000 итераций) с криптостойкой случайной солью (16 байт) на каждую запись.
- Пароли в открытом виде и соли никогда не передаются за пределы слоя `Infrastructure` и строго исключены из любых DTO.

---

## Техническая подноготная и инженерные решения

В процессе разработки был решен ряд нетривиальных архитектурных задач:

### 1. Нативный маппинг кастомных PostgreSQL ENUM без потери производительности
*Проблема:* При использовании стандартных `ValueConverter<Enum, string>` в EF Core драйвер Npgsql передает параметры с типом `text`. PostgreSQL строго типизирован и отказывается выполнять неявный каст `text` в пользовательский ENUM (`error 42804: column is of type uav_aerodynamic_schema but expression is of type text`).  
*Решение:*
- Внедрен `CustomEnumNameTranslator : INpgsqlNameTranslator`, преобразующий C#-идентификаторы (`FixedWing`, `PreFlight`, `Range150gTo30kg`) в точные строковые имена со знаками дефиса и подчеркивания в PostgreSQL (`Fixed-wing`, `Pre-flight`, `150g_to_30kg`).
- Подключение контекста сконфигурировано через `NpgsqlDataSourceBuilder.MapEnum<T>()`, что позволило передавать параметры как родные бинарные ENUM-типы PostgreSQL.

### 2. Поддержка JSONB для расширения характеристик производителями
В таблице `components_catalog` колонка `specifications` сконфигурирована с типом `jsonb`. Это дает нулевую стоимость адаптации: если новый производитель поставляет деталь с уникальными параметрами (разрешение матрицы камеры, частота радиоканала, емкость аккумулятора), эти поля передаются в JSON без изменения схемы БД и без даунтайма.

### 3. Сериализация перечислений в API (System.Text.Json)
По умолчанию сериализатор .NET преобразует перечисления в целые числа. Для удобства работы фронтенда и Swagger в `Program.cs` зарегистрирован `JsonStringEnumConverter()`. Все входные DTO (`UavModelCreateDto`, `UavInstanceCreateDto` и др.) выполнены в виде классов со значениями по умолчанию, что предотвратило ошибки десериализации при частичных JSON-запросах.

### 4. Отказоустойчивое сидирование (`DbInitializer`)
Механизм `DbInitializer.SeedAsync()` выполняет сырой DDL-скрипт с конструкциями `CREATE TABLE IF NOT EXISTS` и проверкой наличия внешних ключей. Это гарантирует, что даже если разработчик запустил Docker с ранее созданным именованным томом (volume), недостающие таблицы аутентификации и внешние ключи добавятся автоматически без необходимости ручного сброса БД.

---

## Интерактивный клиент и Swagger UI

В проект встроен двухэкранный клиент ([`index.html`](file:///c:/Users/shmid/OneDrive/%D0%94%D0%BE%D0%BA%D1%83%D0%BC%D0%B5%D0%BD%D1%82%D1%8B/Desktop/BPLA_DB_it1-main/src/UavFleetManagement.Api/wwwroot/index.html)), раздаваемый сервером по адресу `http://localhost:5000/`:

1. **Экран авторизации (Login Screen):**
   - До момента успешной аутентификации доступ к интерфейсу таблиц полностью скрыт.
   - Поддерживает ручной ввод реквизитов и быстрый вход в один клик под **Superuser** или **User**.
2. **Рабочий кабинет (Main Dashboard):**
   - Открывается после валидации JWT-токена.
   - Отображает текущую роль и логин пользователя.
   - **Таблицы данных с подсветкой Data Masking:** закрытые поля подсвечиваются зеленым для администратора и оранжевым `<скрыто/null>` для обычного пользователя.
   - **Формы отправки данных в БД:** добавление моделей, регистрация бортов и производителей.
   - **Консоль ответов API:** вывод «живого» JSON-ответа сервера с кодами HTTP.
   - **Кнопка «Выйти»:** мгновенный сброс токена и возврат к экрану входа.

Для ручного тестирования и интеграции доступен Swagger UI: `http://localhost:5000/swagger`.

---

## Карта API (REST-контроллеры)

Всего в системе развернуто 12 контроллеров с единым RESTful-стандартом:

| Контроллер | Маршрут | Доступ | Описание |
|---|---|---|---|
| `AuthController` | `/api/auth` | Анонимный / Superuser | Вход и регистрация обычных пользователей и администраторов |
| `ManufacturersController` | `/api/manufacturers` | Чтение: All, Запись: Superuser | Справочник заводов-изготовителей |
| `UavModelsController` | `/api/uavmodels` | Чтение: All (с маской), Запись: Superuser | Справочник моделей БПЛА и ТТХ |
| `UavOwnersController` | `/api/uavowners` | Чтение: All (с маской), Запись: Superuser | Реестр собственников (ФЛ, ИП, ЮЛ) |
| `UavUsersController` | `/api/uavusers` | Чтение: All (с маской), Запись: Superuser | Реестр внешних пилотов и операторов |
| `UserPermissionsController` | `/api/userpermissions` | Чтение: All, Запись: Superuser | Матрица допусков пилотов к моделям (+ `/my`) |
| `UavInstancesController` | `/api/uavinstances` | Чтение: All (с маской), Запись: Superuser | Учет физических бортов парка (+ `PATCH status`) |
| `ComponentsCatalogController` | `/api/componentscatalog` | Чтение: All, Запись: Superuser | Каталог запчастей с JSONB-спецификациями |
| `InstanceComponentsController` | `/api/instancecomponents` | Чтение: All, Запись: Superuser | Монтаж, демонтаж и учет установленных узлов |
| `EngineersController` | `/api/engineers` | Чтение: All (с маской), Запись: Superuser | Справочник специалистов по ТОиР |
| `MaintenanceLogsController` | `/api/maintenancelogs` | Чтение: All, Запись: Superuser | Журнал регламентных работ ТОиР |
| `MaintenanceReplacementsController` | `/api/maintenancereplacements` | Чтение: All, Запись: Superuser | Журнал замен компонентов при ТО |

---

## Инструкция по развертыванию и запуску

### Предварительные требования:
- Установленный **Docker Desktop**
- Установленный **.NET 8 SDK** (или новее)

### Шаг 1. Запуск базы данных в Docker
Откройте консоль в корневой папке проекта и выполните:
```bash
docker compose up -d
```
*(Для полного сброса базы и применения миграций с нуля: `docker compose down -v && docker compose up -d`).*

Контейнер `uav_civil_postgres` поднимется на стандартном порту `5432`.

### Шаг 2. Запуск Backend API
В терминале выполните:
```bash
dotnet run --project src/UavFleetManagement.Api
```

После старта в консоли появится:
```text
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

### Шаг 3. Открытие в браузере
- **Веб-клиент:** [http://localhost:5000/](http://localhost:5000/)
- **Swagger UI:** [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## Тестовые учетные записи

При первом запуске база данных автоматически инициализируется следующими пользователями:

| Роль | Логин | Пароль | Чекбокс «Вход суперпользователя» | Привилегии |
|---|---|---|:---:|---|
| **Superuser** | `admin` | `Admin123!` | ✅ Включен | Полный CRUD, видит цены, полисы, полные ПДн |
| **User** | `pilot` | `Pilot123!` | ❌ Выключен | Чтение каталогов и бортов, скрытые цены, маскированные лицензии |

---

## Сборка и публикация

Для сборки проекта в Release-конфигурации:
```bash
dotnet build -c Release
```

Для создания автономного пакета публикации:
```bash
dotnet publish src/UavFleetManagement.Api -c Release -o ./publish
```
