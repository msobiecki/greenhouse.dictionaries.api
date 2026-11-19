using System.Globalization;

namespace Greenhouse.Dictionaries.Application.Configuration;

public static class LanguageConfiguration
{
    public const string DefaultCulture = "en-US";
    public const string PolishCulture = "pl-PL";

    public static CultureInfo[] CreateSupportedCultures() =>
    [
        CultureInfo.GetCultureInfo(DefaultCulture),
        CultureInfo.GetCultureInfo(PolishCulture)
    ];
}
