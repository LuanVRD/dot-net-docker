using System;
using TaskManager.API.Models;

namespace TaskManager.API.Dtos;

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
