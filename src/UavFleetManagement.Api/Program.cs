using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UavFleetManagement.Api.Middlewares;
using UavFleetManagement.Application.Common.Events;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Infrastructure.Identity;
using UavFleetManagement.Infrastructure.Outbox;
using UavFleetManagement.Infrastructure.Persistence;
using UavFleetManagement.Infrastructure.Persistence.Contexts;
using UavFleetManagement.Infrastructure.Storage;
using UavFleetManagement.Infrastructure.Workers;
using Npgsql;
using UavFleetManagement.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение EF Core к PostgreSQL (из docker-compose.yml: uav_civil_db)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var translator = new CustomEnumNameTranslator();
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

// Доменные перечисления
dataSourceBuilder.MapEnum<UavAerodynamicSchema>("uav_aerodynamic_schema", translator);
dataSourceBuilder.MapEnum<UavWeightCategory>("uav_weight_category", translator);
dataSourceBuilder.MapEnum<UavInstanceStatus>("uav_instance_status", translator);
dataSourceBuilder.MapEnum<MaintenanceType>("maintenance_type", translator);
dataSourceBuilder.MapEnum<OwnerLegalType>("owner_legal_type", translator);
dataSourceBuilder.MapEnum<UavControlType>("uav_control_type", translator);
dataSourceBuilder.MapEnum<ComponentCondition>("component_condition", translator);

// Новые EDA перечисления
dataSourceBuilder.MapEnum<UavApplicationType>("uav_application_type", translator);
dataSourceBuilder.MapEnum<UavApplicationStatus>("uav_application_status", translator);
dataSourceBuilder.MapEnum<InsurancePolicyType>("insurance_policy_type", translator);
dataSourceBuilder.MapEnum<PolicyStatus>("policy_status", translator);

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// 2. Сервисы безопасности, хранилища и событийно-ориентированной архитектуры (EDA)
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IS3StorageService, S3StorageService>();

// Шина событий и фоновые воркеры Outbox/Inbox
builder.Services.AddSingleton<IEventBus, ChannelEventBus>();
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddHostedService<IntegrationEventConsumersWorker>();

// 3. JWT Аутентификация
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "SUPER_SECRET_KEY_FOR_JWT_AUTHENTICATION_MIN_32_CHARS_LONG_12345");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "UavFleetManagementApi",
        ValidAudience = jwtSettings["Audience"] ?? "UavFleetManagementClient",
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero // Строгий срок действия токена 30 минут
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperuserOnly", policy => policy.RequireRole("Superuser", "Admin"));
    options.AddPolicy("StandardUser", policy => policy.RequireRole("User", "Superuser", "Client", "Pilot", "Engineer", "Agent", "Accountant", "Auditor", "Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// 4. Swagger UI с поддержкой ввода JWT Bearer токена
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UAV Fleet Management EDA & Enterprise API",
        Version = "v1",
        Description = "Единое окно БАС: госучет (ПП РФ № 658), расчет ст. 131 ВЗК РФ, ТОиР, S3-хранилище и Transactional Outbox"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен (без слова Bearer)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Инициализация и сидирование данных (если БД доступна)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        if (context.Database.CanConnect())
        {
            await DbInitializer.SeedAsync(context, hasher);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "База данных еще не готова для применения сидов. Запустите docker-compose up -d.");
    }
}

// 5. Конвейер обработки HTTP-запросов
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UAV Fleet API v1");
    c.RoutePrefix = "swagger";
});

// Раздача статических файлов React SPA
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback для React роутинга (исключая /api)
app.MapFallbackToFile("{*path:regex(^(?!api/).*$)}", "index.html");

app.Run();

public partial class Program { }
