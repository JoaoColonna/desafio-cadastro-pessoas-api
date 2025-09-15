using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.Services;
using RegisterAPI.Infrasctructure.Repositories;
using RegisterAPI.Infrasctructure.Database;
using RegisterAPI.Services;
using RegisterAPI.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace RegisterAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AppConstants.CorsPolicy.AllowedOrigins, policy =>
                {
                    policy.WithOrigins(
                        AppConstants.AllowedUrls.VercelApp,
                        AppConstants.AllowedUrls.LocalDevelopment
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var jwtSettings = configuration.GetSection(AppConstants.ConfigurationSections.JwtSettings);
            var secretKey = environment.IsProduction() 
                ? Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.JwtSecretKey) ?? throw new InvalidOperationException("JWT SecretKey não configurada em produção")
                : jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");

            var issuer = environment.IsProduction()
                ? Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.JwtIssuer) ?? AppConstants.Defaults.DefaultIssuer
                : jwtSettings["Issuer"] ?? AppConstants.Defaults.DefaultIssuer;

            var audience = environment.IsProduction()
                ? Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.JwtAudience) ?? AppConstants.Defaults.DefaultAudience
                : jwtSettings["Audience"] ?? AppConstants.Defaults.DefaultAudience;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(AppConstants.Swagger.Version, new OpenApiInfo 
                { 
                    Title = AppConstants.Swagger.Title, 
                    Version = AppConstants.Swagger.Version 
                });
                
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

            return services;
        }

        public static IServiceCollection AddCustomDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = GetConnectionString(configuration, environment);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: AppConstants.DatabasePool.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(AppConstants.DatabasePool.MaxRetryDelaySeconds),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(AppConstants.DatabasePool.CommandTimeout);
                });
                
                // Configurações adicionais para produção
                if (environment.IsProduction())
                {
                    options.EnableSensitiveDataLogging(false);
                    options.EnableDetailedErrors(false);
                }
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar serviços da aplicação
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IPersonServiceV2, PersonServiceV2>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IPersonRepositoryV2, PersonRepositoryV2>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<DatabaseInitializer>();

            return services;
        }

        private static string GetConnectionString(IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (environment.IsProduction())
            {
                var databaseUrl = Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.DatabaseUrl);
                if (string.IsNullOrWhiteSpace(databaseUrl))
                {
                    throw new InvalidOperationException("DATABASE_URL não configurada em produção");
                }

                // Converter URI do PostgreSQL para connection string do .NET com configurações otimizadas
                if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
                {
                    var uri = new Uri(databaseUrl);
                    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true;Pooling=true;MinPoolSize={AppConstants.DatabasePool.MinPoolSize};MaxPoolSize={AppConstants.DatabasePool.MaxPoolSize};Connection Idle Lifetime={AppConstants.DatabasePool.ConnectionIdleLifetime};Connection Pruning Interval={AppConstants.DatabasePool.ConnectionPruningInterval};Command Timeout={AppConstants.DatabasePool.CommandTimeout};Timeout={AppConstants.DatabasePool.ConnectionTimeout}";
                }
                else
                {
                    // Assumir que já está no formato correto, mas adicionar configurações de pool
                    return $"{databaseUrl};Pooling=true;MinPoolSize={AppConstants.DatabasePool.MinPoolSize};MaxPoolSize={AppConstants.DatabasePool.MaxPoolSize};Connection Idle Lifetime={AppConstants.DatabasePool.ConnectionIdleLifetime};Connection Pruning Interval={AppConstants.DatabasePool.ConnectionPruningInterval};Command Timeout={AppConstants.DatabasePool.CommandTimeout};Timeout={AppConstants.DatabasePool.ConnectionTimeout}";
                }
            }
            else
            {
                return configuration.GetConnectionString(AppConstants.ConnectionSettings.DefaultConnection) 
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
        }
    }
}