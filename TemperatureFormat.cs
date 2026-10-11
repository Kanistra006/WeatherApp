using System.Globalization;

namespace WeatherApp;

public static class TemperatureFormat
{
    /// <summary>+18 °C / -3 °C / 0 °C; дробные значения округляются до десятых.</summary>
    public static string Format(double value)
    {
        var rounded = Math.Round(value, 1, MidpointRounding.AwayFromZero);
        if (rounded == 0)
            rounded = 0; // избавляемся от «-0»
        var sign = rounded > 0 ? "+" : "";
        return $"{sign}{rounded.ToString("0.#", CultureInfo.InvariantCulture)} °C";
    }
}
