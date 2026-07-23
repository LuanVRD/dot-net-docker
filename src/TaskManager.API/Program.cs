var builder = WebApplication.CreateBuilder(args);

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

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1");
        options.RoutePrefix = "swagger"; // Exposto em http://localhost:PORT/swagger
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

app.Run();
