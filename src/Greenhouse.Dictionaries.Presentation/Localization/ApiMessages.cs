using System.Globalization;
using System.Resources;
using FluentValidation.Results;

namespace Greenhouse.Dictionaries.Presentation.Localization;

public static class ApiMessages
{
    private static readonly ResourceManager Resources = new(typeof(ApiMessages));

    public static string UnexpectedError => Get(nameof(UnexpectedError));

    public static string InvalidCommand => Get(nameof(InvalidCommand));

    public static string PlantNotFound(int id) => Get(nameof(PlantNotFound), id);

    public static string PlantAlreadyExists(int id) => Get(nameof(PlantAlreadyExists), id);

    public static string SearchTextTooLong => Get(nameof(SearchTextTooLong));

    public static string FamilyFilterTooLong => Get(nameof(FamilyFilterTooLong));

    public static string TemperatureFilterOutOfRange => Get(nameof(TemperatureFilterOutOfRange));

    public static string HumidityFilterOutOfRange => Get(nameof(HumidityFilterOutOfRange));

    public static string ToxicityTargetTooLong => Get(nameof(ToxicityTargetTooLong));

    public static string ValidationError(ValidationFailure error)
    {
        if (error.PropertyName.EndsWith(".ScientificName", StringComparison.Ordinal) ||
            error.PropertyName.EndsWith(".CommonName", StringComparison.Ordinal) ||
            error.PropertyName.EndsWith(".Family", StringComparison.Ordinal) ||
            error.PropertyName.EndsWith(".Description", StringComparison.Ordinal) ||
            error.PropertyName.EndsWith(".IdentifyingCharacteristics", StringComparison.Ordinal))
        {
            return Get("RequiredField", error.PropertyName);
        }

        if (error.ErrorMessage.Contains("must not be empty", StringComparison.OrdinalIgnoreCase) ||
            error.ErrorMessage.Contains("must not be blank", StringComparison.OrdinalIgnoreCase))
        {
            return Get("RequiredField", error.PropertyName);
        }

        var customMessageKey = error.ErrorMessage switch
        {
            "A plant category is required." => "PlantCategoryRequired",
            "Minimum temperature must not exceed maximum temperature." => "TemperatureRangeInvalid",
            "Minimum humidity must not exceed maximum humidity." => "HumidityRangeInvalid",
            "Minimum sunlight hours must not exceed maximum sunlight hours." => "SunlightHoursRangeInvalid",
            "Image URL must be an absolute HTTP or HTTPS URL." => "ImageUrlInvalid",
            "Each toxicity target must be unique." => "ToxicityTargetUnique",
            "Toxicity source URL must be an absolute HTTP or HTTPS URL." => "ToxicitySourceUrlInvalid",
            _ => null
        };

        return customMessageKey is null ? ValidationErrorByCode(error) : Get(customMessageKey);
    }

    private static string ValidationErrorByCode(ValidationFailure error)
    {
        var field = error.PropertyName;
        return error.ErrorCode switch
        {
            "NotEmptyValidator" or "NotNullValidator" => Get("RequiredField", field),
            "MaximumLengthValidator" => Get("MaximumLength", field,
                error.FormattedMessagePlaceholderValues.GetValueOrDefault("MaxLength")),
            "InclusiveBetweenValidator" => Get("InclusiveBetween", field,
                error.FormattedMessagePlaceholderValues.GetValueOrDefault("From"),
                error.FormattedMessagePlaceholderValues.GetValueOrDefault("To")),
            "GreaterThanValidator" => Get("GreaterThan", field,
                error.FormattedMessagePlaceholderValues.GetValueOrDefault("ComparisonValue")),
            "EnumValidator" => Get("EnumInvalid", field),
            "RegularExpressionValidator" => Get("RegexInvalid", field),
            _ => Get("InvalidField", field)
        };
    }

    private static string Get(string name, params object?[] arguments)
    {
        var template = Resources.GetString(name, CultureInfo.CurrentUICulture) ?? name;
        return arguments.Length == 0
            ? template
            : string.Format(CultureInfo.CurrentCulture, template, arguments);
    }
}
