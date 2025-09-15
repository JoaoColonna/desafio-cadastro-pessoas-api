using RegisterAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configurações personalizadas
builder.Services.AddCustomCors();
builder.Services.AddCustomAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddCustomSwagger();
builder.Services.AddCustomDatabase(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();

// Criar aplicação e configurar porta do Heroku
var app = builder.ConfigureHerokuPort();

// Inicializar banco de dados
await app.InitializeDatabaseAsync();

// Configurar pipeline de middleware
app.ConfigureMiddlewarePipeline();

app.Run();
