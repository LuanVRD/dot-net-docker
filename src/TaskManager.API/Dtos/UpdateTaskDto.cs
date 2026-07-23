using TaskManager.API.Models;

namespace TaskManager.API.Dtos;

public record UpdateTaskDto(string Title, string? Description, TaskItemStatus Status);
