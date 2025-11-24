using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation;
using MediatR;
using Amazon.S3;
using RabbitMQ.Client;

// Importações das camadas
using ApiCentral.Infrastructure.Data;
using ApiCentral.Infrastructure.Repositories;
using ApiCentral.Infrastructure.Storage;
using ApiCentral.Infrastructure.Messaging;
using ApiCentral.Infrastructure.Security;
using ApiCentral.Infrastructure.HealthChecks;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Application.DTOs;
using ApiCentral.Application.Validators;
using ApiCentral.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// CORS - política liberada (usar com cuidado em produção)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Entity Framework
var teste = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiCentralDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssemblyContaining<ApiCentral.Application.Handlers.UploadLoteCommandHandler>();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// JWT Authentication
var secretKey = builder.Configuration["JWT:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT:SecretKey não foi configurada no appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// AWS S3
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new Amazon.S3.AmazonS3Config()
    {
        RegionEndpoint = Amazon.RegionEndpoint.USEast1
    };
    
    // Apenas definir ServiceURL se não for null ou vazio
    var serviceUrl = builder.Configuration["AWS:S3:ServiceURL"];
    if (!string.IsNullOrEmpty(serviceUrl))
    {
        config.ServiceURL = serviceUrl;
    }
    
    return new Amazon.S3.AmazonS3Client(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"],
        config
    );
});

// RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost",
        UserName = builder.Configuration["RabbitMQ:UserName"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672")
    };
    return factory.CreateConnection();
});

// Repository dependencies
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPerfilProcessamentoRepository, PerfilProcessamentoRepository>();
builder.Services.AddScoped<ILoteProcessamentoRepository, LoteProcessamentoRepository>();
builder.Services.AddScoped<IProcessamentoLogRepository, ProcessamentoLogRepository>();

// Service dependencies
builder.Services.AddScoped<IStorageService, S3StorageService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Consumer dependencies - DESABILITADO: Worker externo consome as mensagens
// builder.Services.AddSingleton<IMessageConsumer, LoteProcessamentoConsumer>();
// builder.Services.AddHostedService<RabbitMQConsumerService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApiCentralDbContext>("database", tags: new[] { "ready", "database" })
    .AddCheck<RabbitMQHealthCheck>("rabbitmq", tags: new[] { "ready", "messaging" })
    .AddCheck<S3HealthCheck>("s3", tags: new[] { "ready", "storage" })
    .AddCheck<ApplicationHealthCheck>("application", tags: new[] { "live", "application" });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Central - Plataforma PCL",
        Version = "v1",
        Description = "API Central da Gráfica Ltda para processamento de arquivos PCL",
        Contact = new OpenApiContact
        {
            Name = "Gráfica Ltda",
            Email = "contato@graficaltda.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Central v1");
    c.RoutePrefix = "swagger"; // Swagger em /swagger
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

// Middleware personalizado
app.UseMiddleware<ErrorHandlingMiddleware>();

// Habilitar CORS - aplicar política antes de autenticação/autorização
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapControllers();

// Criar banco de dados e fazer seed dos dados iniciais
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApiCentralDbContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
    
    // Criar banco se não existir e fazer seed
    await context.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(context, passwordService);
}

app.Run();
