using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.Dtos;

namespace TaskManager.API.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(string? status);
    Task<TaskResponseDto?> GetTaskByIdAsync(Guid id);
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto);
    Task UpdateTaskAsync(Guid id, UpdateTaskDto dto);
    Task CompleteTaskAsync(Guid id);
    Task DeleteTaskAsync(Guid id);
}
