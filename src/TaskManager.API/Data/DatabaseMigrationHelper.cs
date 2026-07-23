using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskManager.API.Data;

public static class DatabaseMigrationHelper
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        int retries = 5;
        int delayMilliseconds = 3000;

        for (int i = 1; i <= retries; i++)
        {
            try
            {
                logger.LogInformation("Tentando aplicar migrations no banco de dados (Tentativa {Current}/{Total})...", i, retries);
                
                // Aplica as migrations se houver alguma pendente (e cria o banco se não existir)
                context.Database.Migrate();
                
                logger.LogInformation("Banco de dados migrado com sucesso.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Não foi possível conectar ao banco de dados: {Message}", ex.Message);
                
                if (i == retries)
                {
                    logger.LogError("Falha crítica: Excedido número máximo de tentativas de conexão com o banco.");
                    throw;
                }

                logger.LogInformation("Aguardando {Delay}s para tentar novamente...", delayMilliseconds / 1000);
                Thread.Sleep(delayMilliseconds);
            }
        }
    }
}
