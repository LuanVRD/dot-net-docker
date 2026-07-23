using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.API.Dtos;
using TaskManager.API.Models;
using TaskManager.API.Repositories;

namespace TaskManager.API.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(string? status)
    {
        TaskItemStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<TaskItemStatus>(status, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
            else
            {
                throw new ArgumentException($"Status inválido: '{status}'. Use Pending, InProgress ou Completed.");
            }
        }

        var tasks = await _repository.GetAllAsync(statusFilter);

        return tasks.Select(t => new TaskResponseDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status.ToString(),
            t.CreatedAt,
            t.UpdatedAt
        ));
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        
        if (task is null)
        {
            return null;
        }

        return new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.CreatedAt,
            task.UpdatedAt
        );
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("O título é obrigatório e não pode ser vazio.");
        }

        if (dto.Title.Length > 150)
        {
            throw new ArgumentException("O título não pode exceder 150 caracteres.");
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Status = TaskItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(task);

        return new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.CreatedAt,
            task.UpdatedAt
        );
    }

    public async Task UpdateTaskAsync(Guid id, UpdateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("O título é obrigatório e não pode ser vazio.");
        }

        if (dto.Title.Length > 150)
        {
            throw new ArgumentException("O título não pode exceder 150 caracteres.");
        }

        var task = await _repository.GetByIdAsync(id);

        if (task is null)
        {
            throw new KeyNotFoundException($"Tarefa com ID {id} não foi encontrada.");
        }

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(task);
    }

    public async Task CompleteTaskAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task is null)
        {
            throw new KeyNotFoundException($"Tarefa com ID {id} não foi encontrada.");
        }

        task.Status = TaskItemStatus.Completed;
        task.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);

        if (task is null)
        {
            throw new KeyNotFoundException($"Tarefa com ID {id} não foi encontrada.");
        }

        await _repository.DeleteAsync(task);
    }
}
