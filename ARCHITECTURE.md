# Documentação Técnica de Referência: TaskManager API

Este documento serve como especificação técnica detalhada do projeto **TaskManager API** para guiar o desenvolvimento do sistema e servir como referência para a Inteligência Artificial e para o desenvolvedor.

---

## 1. Visão Geral e Objetivos

O **TaskManager API** é uma API REST desenvolvida em **ASP.NET Core 9.0** com persistência em banco de dados **PostgreSQL** rodando sob containers **Docker**. O propósito primordial é o aprendizado prático de:
1. Modelagem e desenvolvimento de APIs RESTful resilientes com .NET.
2. Mapeamento relacional com Entity Framework Core.
3. Criação de imagens Docker otimizadas (multi-stage build).
4. Comunicação entre múltiplos containers via Docker Compose (Redes, Volumes e Variáveis de Ambiente).

---

## 2. Tecnologias Utilizadas

- **Backend**: .NET 9.0 SDK (C# 13)
- **Banco de Dados**: PostgreSQL 16
- **ORM**: Entity Framework Core 9.0
- **Documentação**: OpenAPI (Swagger) integrada
- **Containerização**: Docker e Docker Compose

---

## 3. Estrutura de Pastas e Componentes

A estrutura física do repositório será organizada da seguinte forma:

```text
/dot-net-docker
├── ARCHITECTURE.md          # Este documento de referência técnica
├── docker-compose.yml       # Orquestração do ambiente multi-container
├── .dockerignore            # Filtro de arquivos enviados ao contexto Docker
└── src/
    └── TaskManager.API/
        ├── TaskManager.API.csproj
        ├── Program.cs       # Configuração e definição de endpoints
        ├── Dockerfile       # Instruções de compilação da imagem docker
        ├── appsettings.json
        ├── Data/
        │   ├── AppDbContext.cs            # Contexto EF Core
        │   └── DatabaseMigrationHelper.cs # Inicializador de migração do banco
        ├── Models/
        │   ├── TaskItem.cs   # Entidade principal
        │   └── TaskStatus.cs # Enum de status da tarefa
        └── Dtos/
            ├── CreateTaskDto.cs
            ├── UpdateTaskDto.cs
            └── TaskResponseDto.cs
```

---

## 4. Modelagem de Dados

### Tabela: `Tasks`
A modelagem utilizará tipos primitivos do Postgres correspondentes aos tipos nativos do C#.

| Campo | Tipo C# | Tipo PostgreSQL | Restrições | Descrição |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `Guid` | `uuid` | Primary Key, Gerado pelo App | Identificador único da tarefa |
| `Title` | `string` | `varchar(150)` | NOT NULL | Título descritivo da tarefa |
| `Description` | `string?` | `text` | NULL | Detalhes adicionais |
| `Status` | `TaskStatus` (Enum) | `integer` (ou string) | NOT NULL, Default: `0` (Pending) | Status atual |
| `CreatedAt` | `DateTime` | `timestamp with time zone` | NOT NULL | Data/hora de criação |
| `UpdatedAt` | `DateTime?` | `timestamp with time zone` | NULL | Data/hora da última atualização |

### Enum: `TaskStatus`
- `0` - `Pending` (Pendente)
- `1` - `InProgress` (Em progresso)
- `2` - `Completed` (Concluída)

---

## 5. Especificação de Endpoints (Contrato REST)

A API seguirá os princípios semânticos do protocolo HTTP, fornecendo retornos de erro formatados quando apropriado. O prefixo base das rotas é `/api/tasks`.

### 5.1 Criar Tarefa
- **Rota**: `POST /api/tasks`
- **Request Body** (`CreateTaskDto`):
  ```json
  {
    "title": "Aprender Docker Compose",
    "description": "Criar um arquivo docker-compose.yml com banco e aplicação .NET"
  }
  ```
- **Responses**:
  - `201 Created`: Retorna a tarefa criada com o Header `Location` apontando para o GET da tarefa.
    ```json
    {
      "id": "e4f8d55c-1647-4903-bca9-db19fca951e7",
      "title": "Aprender Docker Compose",
      "description": "Criar um arquivo docker-compose.yml com banco e aplicação .NET",
      "status": "Pending",
      "createdAt": "2026-07-23T12:00:00Z",
      "updatedAt": null
    }
    ```
  - `400 Bad Request`: Título ausente, nulo ou maior que 150 caracteres.

### 5.2 Listar Tarefas
- **Rota**: `GET /api/tasks`
- **Query Parameters**:
  - `status` (opcional): Filtra por status (`Pending`, `InProgress`, `Completed`).
- **Response**:
  - `200 OK`: Array de tarefas.
    ```json
    [
      {
        "id": "e4f8d55c-1647-4903-bca9-db19fca951e7",
        "title": "Aprender Docker Compose",
        "description": "Criar um arquivo docker-compose.yml...",
        "status": "Pending",
        "createdAt": "2026-07-23T12:00:00Z",
        "updatedAt": null
      }
    ]
    ```

### 5.3 Obter Tarefa por ID
- **Rota**: `GET /api/tasks/{id}`
- **Response**:
  - `200 OK`: Dados da tarefa correspondente.
  - `404 Not Found`: Identificador inexistente.

### 5.4 Atualizar Tarefa Completa
- **Rota**: `PUT /api/tasks/{id}`
- **Request Body** (`UpdateTaskDto`):
  ```json
  {
    "title": "Aprender Docker Compose Avançado",
    "description": "Configurar healthchecks e volumes no Compose",
    "status": "InProgress"
  }
  ```
- **Response**:
  - `204 No Content`: Atualizado com sucesso.
  - `400 Bad Request`: Payload inválido.
  - `404 Not Found`: Identificador inexistente.

### 5.5 Concluir Tarefa (Ação Rápida)
- **Rota**: `PATCH /api/tasks/{id}/complete`
- **Response**:
  - `204 No Content`: Marcado como `Completed` e `UpdatedAt` atualizado.
  - `404 Not Found`: Identificador inexistente.

### 5.6 Remover Tarefa
- **Rota**: `DELETE /api/tasks/{id}`
- **Response**:
  - `204 No Content`: Removido com sucesso.
  - `404 Not Found`: Identificador inexistente.

---

## 6. Infraestrutura e Docker

### Dockerfile (Aplicação .NET)
Será criado na raiz da aplicação (`src/TaskManager.API/Dockerfile`). Utilizará o conceito de compilação multi-estágio (`multi-stage build`):
1. **Build e Restore**: Baseado na imagem oficial `mcr.microsoft.com/dotnet/sdk:9.0`. Copia os projetos, restaura pacotes e publica os binários compilados em release.
2. **Execução**: Baseado na imagem enxuta `mcr.microsoft.com/dotnet/aspnet:9.0`. Roda a aplicação otimizada sob uma porta não-privilegiada (por exemplo, 8080) e com usuário não-root por motivos de segurança.

### Docker Compose (`docker-compose.yml`)
Configuração na raiz do repositório contendo dois serviços principais:

1. **`db` (PostgreSQL)**:
   - Imagem: `postgres:16-alpine` (versão estável e leve).
   - Portas: Não exposta ao host externo por padrão (apenas via rede interna do docker), ou exposta na porta `5432` para debugging local via DBeaver/PgAdmin.
   - Variáveis de Ambiente: `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`.
   - Volumes: `postgres_data:/var/lib/postgresql/data` (Garante persistência de dados fora do ciclo de vida do container).
   - Healthcheck: Executa `pg_isready` para sinalizar quando o banco está pronto para aceitar conexões.

2. **`web` (API ASP.NET Core)**:
   - Build: Aponta para a pasta `src/TaskManager.API`.
   - Portas: Mapeia a porta `8080` do container para `8080` do host local (acesso via browser em `http://localhost:8080/swagger`).
   - Variáveis de Ambiente:
     - `ConnectionStrings__DefaultConnection`: String de conexão apontando para o serviço de banco (`Host=db;Database=taskdb;Username=postgres;Password=...`).
     - `ASPNETCORE_ENVIRONMENT`: `Development` (para habilitar o Swagger na inicialização dentro do container).
   - Dependência: `depends_on` configurado com `condition: service_healthy` em relação ao container `db`.
