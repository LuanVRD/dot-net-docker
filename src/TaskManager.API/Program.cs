using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Middlewares;
using TaskManager.API.Repositories;
using TaskManager.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Registro dos Controllers com configuração global de serialização de Enums como Strings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Registro das Camadas do Aplicativo (Injeção de Dependência)
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Registro do DbContext com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro do Handler Global de Exceções (.NET 9)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configuração do Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TaskManager API",
        Version = "v1",
        Description = "API REST de gerenciamento de tarefas desenvolvida para estudos de .NET 9 e Docker."
    });
});

var app = builder.Build();

// Ativa o middleware de tratamento de exceções global
app.UseExceptionHandler();

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1");
        options.RoutePrefix = "swagger";
    });
}

// Rota padrão para verificação de status da API
app.MapGet("/", () => Results.Ok(new 
{ 
    Message = "TaskManager API está ativa e rodando!", 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow 
}))
.WithName("GetStatus")
.WithOpenApi();

// Mapeamento das rotas dos Controllers
app.MapControllers();

// Aplica migrações automáticas se não for ambiente de CLI/Build
if (!EF.IsDesignTime)
{
    app.ApplyMigrations();
}

app.Run();
