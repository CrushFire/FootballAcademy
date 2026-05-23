using Application.Services;
using Application.Services.Coefficientes;
using Application.Services.MetricAnalytic;
using Application.Utils;
using AutoMapper;
using Core.Interfaces.Services;
using Core.Models.Config;
using DataAccess;
using FootballAcademy.Hubs;
using FootballAcademy.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Text;

// EPPlus license configuration
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// PostgreSQL: все DateTime трактуем как UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Подключаем отдельный файл коэффициентов. Изменения требуют рестарта бэка (hot-reload не включён).
builder.Configuration.AddJsonFile("appsettings.Coefficients.json", optional: false, reloadOnChange: false);

// Загружаем коэффициенты в статический провайдер, чтобы static-сервисы Coefficientes их видели.
var coefficientsConfig = new CoefficientsConfig();
builder.Configuration.Bind(coefficientsConfig);
CoefficientsConfigProvider.Initialize(coefficientsConfig);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add AutoMapper
// ВАЖНО: Версии AutoMapper 12.0.1 и AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1 должны совпадать!
// НЕ ОБНОВЛЯТЬ до AutoMapper 16+ - несовместимо с Extensions 12.x
builder.Services.AddAutoMapper(typeof(FootballAcademy.Mappers.AutoMapper));



// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Введите свой токен",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add PostgreSQL + pgvector
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        o => o.UseVector()
    ));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authorizationHeader) &&
                    authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    context.Token = authorizationHeader["Bearer ".Length..].Trim();

                // SignalR (WebSocket) передаёт токен через query string ?access_token=...
                // потому что нативный WebSocket не поддерживает кастомные заголовки.
                // Это позволяет ChatHub.Context.User получить идентификацию пользователя.
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddScoped<JwtTokenGen>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISportsmanService, SportsmanService>();
builder.Services.AddScoped<IPersonalService, PersonalService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<INormativeService, NormativeService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISchedualeService, SchedualeService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<ParserMetrics>();
builder.Services.AddScoped<IMetricsService, MetricService>();
builder.Services.AddSingleton<MetricAutoImportService>();
builder.Services.AddSingleton<MatchCleanupService>();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IMainMetricService, MainMetricService>();
builder.Services.AddScoped<IMedicalMetricService, MedicalMetricService>();
builder.Services.AddScoped<IPentagonService, PentagonService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddSingleton<ArticleKnowledgeService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ArticleKnowledgeService>());
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Remove HTTPS redirect for Docker
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

// Запускаем фоновую очистку брошенных матчей
app.Services.GetRequiredService<MatchCleanupService>().Start();

app.Run();
