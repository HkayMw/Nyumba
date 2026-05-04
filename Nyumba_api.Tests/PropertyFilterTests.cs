using Nyumba_api.Models.Entities;
using Nyumba_api.Services;
using Xunit;

namespace Nyumba_api.Tests;

public class PropertyFilterTests
{
    [Fact]
    public void ApplyFiltersUsesDistrictAndCityIndependently()
    {
        var matchingProperty = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Area 10 apartment",
            Price = 750,
            City = "Lilongwe",
            District = "Central",
            IsAvailable = true,
            OwnerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var properties = new List<Property>
        {
            matchingProperty,
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Same city, wrong district",
                Price = 800,
                City = "Lilongwe",
                District = "Northern",
                IsAvailable = true,
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Same district, wrong city",
                Price = 650,
                City = "Blantyre",
                District = "Central",
                IsAvailable = true,
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Unavailable match",
                Price = 600,
                City = "Lilongwe",
                District = "Central",
                IsAvailable = false,
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var result = PropertyService.ApplyFilters(
            properties,
            minPrice: null,
            maxPrice: null,
            title: null,
            district: "Central",
            city: "Lilongwe",
            propertyType: null,
            bedrooms: null,
            bathrooms: null,
            minSquareFeet: null,
            maxSquareFeet: null).ToList();

        var property = Assert.Single(result);
        Assert.Equal(matchingProperty.Id, property.Id);
    }

    [Fact]
    public void ApplyFiltersExcludesUnavailableProperties()
    {
        var properties = new List<Property>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Available",
                Price = 500,
                IsAvailable = true,
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Unavailable",
                Price = 500,
                IsAvailable = false,
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable();

        var result = PropertyService.ApplyFilters(
            properties,
            minPrice: null,
            maxPrice: null,
            title: null,
            district: null,
            city: null,
            propertyType: null,
            bedrooms: null,
            bathrooms: null,
            minSquareFeet: null,
            maxSquareFeet: null).ToList();

        var property = Assert.Single(result);
        Assert.True(property.IsAvailable);
    }
}
