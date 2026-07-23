using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using TaskManager.API.Dtos;
using TaskManager.API.Models;
using TaskManager.API.Repositories;
using TaskManager.API.Services;
using Xunit;

namespace TaskManager.API.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _repositoryMock;
    private readonly TaskService _sut; // SUT = System Under Test (o serviço sendo testado)

    public TaskServiceTests()
    {
        _repositoryMock = Substitute.For<ITaskRepository>();
        _sut = new TaskService(_repositoryMock);
    }

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ReturnsCreatedTaskDto()
    {
        // Arrange
        var dto = new CreateTaskDto(
            Title: "Aprender Docker", 
            Description: "Configurar containers Compose"
        );

        // Act
        var result = await _sut.CreateTaskAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Aprender Docker");
        result.Description.Should().Be("Configurar containers Compose");
        result.Status.Should().Be(TaskItemStatus.Pending);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.UpdatedAt.Should().BeNull();

        // Verifica se o método AddAsync foi chamado no repositório com o objeto correto
        await _repositoryMock.Received(1).AddAsync(Arg.Is<TaskItem>(t => 
            t!.Title == dto.Title && 
            t.Description == dto.Description && 
            t.Status == TaskItemStatus.Pending));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateTaskAsync_WithEmptyOrNullTitle_ThrowsArgumentException(string? title)
    {
        // Arrange
        var dto = new CreateTaskDto(
            Title: title!, 
            Description: "Descrição válida"
        );

        // Act
        Func<Task> act = async () => await _sut.CreateTaskAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O título é obrigatório e não pode ser vazio.");

        // Garante que nenhum registro foi adicionado no banco
        await _repositoryMock.DidNotReceive().AddAsync(Arg.Any<TaskItem>());
    }

    [Fact]
    public async Task CreateTaskAsync_WithTitleTooLong_ThrowsArgumentException()
    {
        // Arrange
        var titleLong = new string('A', 151); // 151 caracteres
        var dto = new CreateTaskDto(
            Title: titleLong, 
            Description: "Descrição válida"
        );

        // Act
        Func<Task> act = async () => await _sut.CreateTaskAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O título não pode exceder 150 caracteres.");

        await _repositoryMock.DidNotReceive().AddAsync(Arg.Any<TaskItem>());
    }

    #endregion

    #region GetTaskByIdAsync Tests

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskExists_ReturnsTaskResponseDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Testar código",
            Description = "Escrever testes unitários",
            Status = TaskItemStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.GetByIdAsync(taskId).Returns(task);

        // Act
        var result = await _sut.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        result.Title.Should().Be("Testar código");
        result.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _repositoryMock.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act
        var result = await _sut.GetTaskByIdAsync(taskId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CompleteTaskAsync Tests

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskExists_UpdatesStatusToCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Task Antiga",
            Status = TaskItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.GetByIdAsync(taskId).Returns(task);

        // Act
        await _sut.CompleteTaskAsync(taskId);

        // Assert
        task.Status.Should().Be(TaskItemStatus.Completed);
        task.UpdatedAt.Should().NotBeNull();
        await _repositoryMock.Received(1).UpdateAsync(task);
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _repositoryMock.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act
        Func<Task> act = async () => await _sut.CompleteTaskAsync(taskId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Tarefa com ID {taskId} não foi encontrada.");

        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<TaskItem>());
    }

    #endregion

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskExists_CallsRepositoryDelete()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = taskId,
            Title = "Remover",
            Status = TaskItemStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.GetByIdAsync(taskId).Returns(task);

        // Act
        await _sut.DeleteTaskAsync(taskId);

        // Assert
        await _repositoryMock.Received(1).DeleteAsync(task);
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _repositoryMock.GetByIdAsync(taskId).Returns((TaskItem?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteTaskAsync(taskId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Tarefa com ID {taskId} não foi encontrada.");

        await _repositoryMock.DidNotReceive().DeleteAsync(Arg.Any<TaskItem>());
    }

    #endregion
}
