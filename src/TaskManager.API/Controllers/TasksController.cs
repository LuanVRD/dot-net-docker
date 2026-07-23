using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.Dtos;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // POST: api/tasks
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var task = await _taskService.CreateTaskAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    // GET: api/tasks
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var tasks = await _taskService.GetAllTasksAsync(status);
        return Ok(tasks);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);

        if (task is null)
        {
            return NotFound(new { Error = $"Tarefa com ID {id} não encontrada." });
        }

        return Ok(task);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTaskDto dto)
    {
        await _taskService.UpdateTaskAsync(id, dto);
        return NoContent();
    }

    // PATCH: api/tasks/{id}/complete
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Complete([FromRoute] Guid id)
    {
        await _taskService.CompleteTaskAsync(id);
        return NoContent();
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _taskService.DeleteTaskAsync(id);
        return NoContent();
    }
}
