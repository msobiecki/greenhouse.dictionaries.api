using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Domain.Exceptions;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Domain;

public sealed class PlantDomainTests
{
    [Fact]
    public void PlantNotFoundExceptionContainsPlantIdAndMessage()
    {
        var exception = new PlantNotFoundException(42);

        Assert.Equal(42, exception.PlantId);
        Assert.Contains("42", exception.Message);
        Assert.IsAssignableFrom<BaseException>(exception);
    }

    [Fact]
    public void PlantAlreadyExistsExceptionContainsPlantIdAndMessage()
    {
        var exception = new PlantAlreadyExistsException(13);

        Assert.Equal(13, exception.PlantId);
        Assert.Contains("13", exception.Message);
        Assert.IsAssignableFrom<BaseException>(exception);
    }

    [Fact]
    public void PlantIsAuditableEntity()
    {
        var plant = new Plant
        {
            ScientificName = "Rosa rugosa",
            CommonName = "Rugosa rose",
            Family = "Rosaceae",
            Category = "flower",
            Description = "A flowering shrub.",
            Sunlight = "Full sun",
            Watering = "Regular"
        };

        Assert.IsAssignableFrom<Greenhouse.Dictionaries.Domain.Abstractions.IAuditableEntity>(plant);
        Assert.Equal(0, plant.Id);
        Assert.Equal(default, plant.CreatedAt);
        Assert.Equal(default, plant.UpdatedAt);
    }

    [Fact]
    public void BaseExceptionPreservesInnerException()
    {
        var innerException = new InvalidOperationException("inner");
        var exception = new TestBaseException("outer", innerException);

        Assert.Equal("outer", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    private sealed class TestBaseException(string message, Exception innerException)
        : BaseException(message, innerException);
}
