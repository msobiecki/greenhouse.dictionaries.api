using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Queries;
using Greenhouse.Dictionaries.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;

namespace Greenhouse.Dictionaries.Presentation.Controllers;

[ApiController]
[Route("api/plants")]
public sealed class PlantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Discover plants", Description = "Search plant names, aliases and descriptions, then filter by category, family, care requirements or suitable environment.")]
    [ProducesResponseType(typeof(IEnumerable<PlantDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? family,
        [FromQuery] SunlightRequirement? sunlightRequirement,
        [FromQuery] WateringRequirement? wateringRequirement,
        [FromQuery] string? toxicityTarget,
        [FromQuery] ToxicityLevel? toxicityLevel,
        [FromQuery] decimal? temperatureC,
        [FromQuery] decimal? humidityPercent,
        CancellationToken cancellationToken)
    {
        if (search?.Length > 200)
        {
            return BadRequest("Search text cannot exceed 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(family) && family.Length > 200)
        {
            return BadRequest("Family filter cannot exceed 200 characters.");
        }

        if (temperatureC is < -50 or > 60)
        {
            return BadRequest("Temperature filter must be between -50 and 60 degrees Celsius.");
        }

        if (humidityPercent is < 0 or > 100)
        {
            return BadRequest("Humidity filter must be between 0 and 100 percent.");
        }

        if (toxicityTarget?.Length > 100)
        {
            return BadRequest("Toxicity target cannot exceed 100 characters.");
        }

        var filters = new PlantSearchFilters(
            Family: family,
            SunlightRequirement: sunlightRequirement,
            WateringRequirement: wateringRequirement,
            ToxicityTarget: toxicityTarget,
            ToxicityLevel: toxicityLevel,
            TemperatureC: temperatureC,
            HumidityPercent: humidityPercent,
            Category: category);
        return Ok(await mediator.Send(new GetPlantsQuery(search, filters), cancellationToken));
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
    [Authorize(Policy = "CatalogWrite")]
    [SwaggerOperation(Summary = "Add a new plant", Description = "Create a new plant with the provided details.")]
    [ProducesResponseType(typeof(PlantDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<IActionResult> Add([FromBody] PlantInputDto plant, CancellationToken cancellationToken)
    {
        var createdPlant = await mediator.Send(new AddPlantCommand(plant), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdPlant.Id }, createdPlant);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CatalogWrite")]
    [SwaggerOperation(Summary = "Update a plant", Description = "Update the details of an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] PlantInputDto plant,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePlantCommand(id, plant), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CatalogWrite")]
    [SwaggerOperation(Summary = "Delete a plant", Description = "Delete an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePlantCommand(id), cancellationToken);
        return NoContent();
    }
}
