using Greenhouse.Dictionaries.Infrastructure.Config.Converters;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Configuration.Converters;

public sealed class DateOnlyConverterTests
{
    [Fact]
    public void ConvertsDateOnlyToMidnightDateTimeAndBack()
    {
        var converter = new DateOnlyConverter();
        var date = new DateOnly(2026, 11, 19);

        var providerValue = converter.ConvertToProvider(date);
        var modelValue = converter.ConvertFromProvider(providerValue);

        Assert.Equal(new DateTime(2026, 11, 19, 0, 0, 0), providerValue);
        Assert.Equal(date, modelValue);
    }
}
