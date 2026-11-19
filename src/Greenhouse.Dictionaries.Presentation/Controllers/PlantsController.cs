using Greenhouse.Dictionaries.Presentation.Localization;
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
using System.Globalization;

namespace Greenhouse.Dictionaries.Presentation.Controllers;

[ApiController]
[Route("api/plants")]
public sealed class PlantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Discover plants", Description = "Search plant names, aliases and descriptions, then filter by category, family, care requirements or suitable environment.")]
    [ProducesResponseType(typeof(PagedResult<PlantDto>), (int)HttpStatusCode.OK)]
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
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        [FromQuery] bool onlyRequestedLanguage = false,
        CancellationToken cancellationToken = default)
    {
        if (search?.Length > 200)
        {
            return BadRequest(ApiMessages.SearchTextTooLong);
        }

        if (!string.IsNullOrWhiteSpace(family) && family.Length > 200)
        {
            return BadRequest(ApiMessages.FamilyFilterTooLong);
        }

        if (temperatureC is < -50 or > 60)
        {
            return BadRequest(ApiMessages.TemperatureFilterOutOfRange);
        }

        if (humidityPercent is < 0 or > 100)
        {
            return BadRequest(ApiMessages.HumidityFilterOutOfRange);
        }

        if (toxicityTarget?.Length > 100)
        {
            return BadRequest(ApiMessages.ToxicityTargetTooLong);
        }

        if (offset < 0)
        {
            return BadRequest(ApiMessages.OffsetMustBeNonNegative);
        }

        if (limit is < 1 or > 100)
        {
            return BadRequest(ApiMessages.LimitOutOfRange);
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
        var plants = await mediator.Send(
            new GetPlantsQuery(
                search,
                filters,
                CultureInfo.CurrentUICulture.Name,
                offset,
                limit,
                onlyRequestedLanguage),
            cancellationToken);
        SetContentLanguages(plants.Items.Select(plant => plant.Language));
        return Ok(plants);
    }

    [HttpGet("{uid:guid}")]
    [SwaggerOperation(Summary = "Get plant by UID", Description = "Retrieve a single plant by its random public UUID.")]
    [ProducesResponseType(typeof(PlantDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetByUid([FromRoute] Guid uid, CancellationToken cancellationToken)
    {
        var plant = await mediator.Send(new GetPlantByIdQuery(uid, CultureInfo.CurrentUICulture.Name), cancellationToken);
        if (plant is not null)
        {
            SetContentLanguages([plant.Language]);
        }
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
        var createdPlant = await mediator.Send(
            new AddPlantCommand(plant, CultureInfo.CurrentUICulture.Name), cancellationToken);
        SetContentLanguages([createdPlant.Language]);
        return CreatedAtAction(nameof(GetByUid), new { uid = createdPlant.Uid }, createdPlant);
    }

    [HttpPut("{uid:guid}")]
    [Authorize(Policy = "CatalogWrite")]
    [SwaggerOperation(Summary = "Update a plant", Description = "Update the details of an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid uid,
        [FromBody] PlantInputDto plant,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdatePlantCommand(uid, plant, CultureInfo.CurrentUICulture.Name), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{uid:guid}")]
    [Authorize(Policy = "CatalogWrite")]
    [SwaggerOperation(Summary = "Delete a plant", Description = "Delete an existing plant by its ID.")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid uid, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePlantCommand(uid), cancellationToken);
        return NoContent();
    }

    private void SetContentLanguages(IEnumerable<string> languages)
    {
        if (ControllerContext?.HttpContext is null)
        {
            return;
        }

        var values = languages.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (values.Length > 0)
        {
            Response.Headers.ContentLanguage = string.Join(", ", values);
        }
    }
}
