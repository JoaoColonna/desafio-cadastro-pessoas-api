using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.Services;
using RegisterAPI.Infrasctructure.Repositories;
using RegisterAPI.Infrasctructure.Database;
using RegisterAPI.Middleware;
using RegisterAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar porta para Heroku
if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configurar JWT - usar variáveis de ambiente APENAS em produção
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = builder.Environment.IsProduction() 
    ? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT SecretKey não configurada em produção")
    : jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");

var issuer = builder.Environment.IsProduction()
    ? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "RegisterAPI"
    : jwtSettings["Issuer"] ?? "RegisterAPI";

var audience = builder.Environment.IsProduction()
    ? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "RegisterAPI"
    : jwtSettings["Audience"] ?? "RegisterAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Adicionar Swagger UI com configuração JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RegisterAPI", Version = "v1" });
    
    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
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

// Configurar connection string
string connectionString;

if (builder.Environment.IsProduction())
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        throw new InvalidOperationException("DATABASE_URL não configurada em produção");
    }

    // Converter URI do PostgreSQL para connection string do .NET
    if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
    {
        var uri = new Uri(databaseUrl);
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        // Assumir que já está no formato correto
        connectionString = databaseUrl;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar serviços
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IPersonServiceV2, PersonServiceV2>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IPersonRepositoryV2, PersonRepositoryV2>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<DatabaseInitializer>();

var app = builder.Build();

// Adicionar middleware de tratamento global de erros
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Inicializar banco de dados
try
{
    using var scope = app.Services.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await dbInitializer.InitializeDatabaseAsync();
    app.Logger.LogInformation("Database initialized successfully.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error initializing database: {ErrorMessage}", ex.Message);
    throw;
}

// Configure the HTTP request pipeline - Swagger habilitado também em produção
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RegisterAPI v1");
    c.RoutePrefix = "swagger"; // Swagger disponível em /swagger
});

// HTTPS Redirect apenas em desenvolvimento (Heroku já gerencia HTTPS)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Importante: A ordem importa!
app.UseAuthentication(); // Deve vir antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
