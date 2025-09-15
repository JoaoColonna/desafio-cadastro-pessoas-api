namespace RegisterAPI.Configuration
{
    public static class AppConstants
    {
        public static class CorsPolicy
        {
            public const string AllowedOrigins = "AllowedOrigins";
        }

        public static class AllowedUrls
        {
            public const string VercelApp = "https://desafio-cadastro-pessoas-front.vercel.app";
            public const string LocalDevelopment = "http://localhost:3000";
        }

        public static class EnvironmentVariables
        {
            public const string Port = "PORT";
            public const string DatabaseUrl = "DATABASE_URL";
            public const string JwtSecretKey = "JWT_SECRET_KEY";
            public const string JwtIssuer = "JWT_ISSUER";
            public const string JwtAudience = "JWT_AUDIENCE";
        }

        public static class ConfigurationSections
        {
            public const string JwtSettings = "JwtSettings";
        }

        public static class ConnectionSettings
        {
            public const string DefaultConnection = "DefaultConnection";
        }

        public static class Swagger
        {
            public const string Version = "v1";
            public const string Title = "RegisterAPI";
            public const string RoutePrefix = "swagger";
            public const string JsonEndpoint = "/swagger/v1/swagger.json";
            public const string DisplayName = "RegisterAPI v1";
        }

        public static class DatabasePool
        {
            public const int MaxPoolSize = 20;
            public const int MinPoolSize = 0;
            public const int ConnectionIdleLifetime = 300;
            public const int ConnectionPruningInterval = 10;
            public const int CommandTimeout = 60;
            public const int ConnectionTimeout = 60;
            public const int MaxRetryCount = 3;
            public const int MaxRetryDelaySeconds = 5;
        }

        public static class Defaults
        {
            public const string DefaultPort = "8080";
            public const string DefaultIssuer = "RegisterAPI";
            public const string DefaultAudience = "RegisterAPI";
        }
    }
}