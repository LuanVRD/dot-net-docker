# Estágio 1: Build da aplicação utilizando o SDK do .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivo de projeto e restaurar dependências
COPY ["TaskManager.API.csproj", "./"]
RUN dotnet restore "TaskManager.API.csproj"

# Copiar todo o restante do código e compilar a aplicação
COPY . .
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Imagem final leve baseada no runtime do ASP.NET Core 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# O ASP.NET 9 roda por padrão sob o usuário não-root "app" na porta 8080
USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
