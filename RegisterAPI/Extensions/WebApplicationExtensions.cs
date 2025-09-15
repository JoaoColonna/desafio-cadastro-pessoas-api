using RegisterAPI.Infrasctructure.Database;
using RegisterAPI.Configuration;

namespace RegisterAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication ConfigureHerokuPort(this WebApplicationBuilder builder)
        {
            if (builder.Environment.IsProduction())
            {
                var port = Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.Port) ?? AppConstants.Defaults.DefaultPort;
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }

            return builder.Build();
        }

        public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
        {
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

            return app;
        }

        public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
        {
            // Adicionar middleware de tratamento global de erros
            app.UseMiddleware<RegisterAPI.Middleware.GlobalExceptionHandlerMiddleware>();

            // CORS deve vir ANTES de Authentication/Authorization
            app.UseCors(AppConstants.CorsPolicy.AllowedOrigins);

            // Swagger habilitado também em produção
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(AppConstants.Swagger.JsonEndpoint, AppConstants.Swagger.DisplayName);
                c.RoutePrefix = AppConstants.Swagger.RoutePrefix;
            });

            // HTTPS Redirect apenas em desenvolvimento (Heroku já gerencia HTTPS)
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Importante: A ordem importa!
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}