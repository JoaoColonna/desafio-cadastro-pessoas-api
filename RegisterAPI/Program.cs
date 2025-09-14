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

// Configurar connection string - usar variável de ambiente DATABASE_URL APENAS em produção
var connectionString = builder.Environment.IsProduction()
    ? Environment.GetEnvironmentVariable("DATABASE_URL") ?? throw new InvalidOperationException("DATABASE_URL não configurada em produção")
    : builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Importante: A ordem importa!
app.UseAuthentication(); // Deve vir antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
