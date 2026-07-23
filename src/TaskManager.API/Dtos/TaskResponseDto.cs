using System;

namespace TaskManager.API.Dtos;

public record TaskResponseDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
