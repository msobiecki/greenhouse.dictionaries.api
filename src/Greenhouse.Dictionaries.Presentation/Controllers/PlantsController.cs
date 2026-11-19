using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;

namespace Greenhouse.Dictionaries.Presentation.Controllers;

[ApiController]
[Route("api/plants")]
public sealed class PlantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all plants", Description = "Retrieve a list of all plants with optional search and category filters.")]
    [ProducesResponseType(typeof(IEnumerable<PlantDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPlantsQuery(search, category), cancellationToken));
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get plant by ID", Description = "Retrieve a single plant by its unique ID.")]
    [ProducesResponseType(typeof(PlantDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var plant = await mediator.Send(new GetPlantByIdQuery(id), cancellationToken);
        return plant is null ? NotFound() : Ok(plant);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Add a new plant", Description = "Create a new plant with the provided details.")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public async Task<IActionResult> Add([FromBody] PlantInputDto plant, CancellationToken cancellationToken)
    {
        var createdPlant = await mediator.Send(new AddPlantCommand(plant), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdPlant.Id }, createdPlant);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update a plant", Description = "Update the details of an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] PlantInputDto plant,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePlantCommand(id, plant), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a plant", Description = "Delete an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePlantCommand(id), cancellationToken);
        return NoContent();
    }
}
