namespace WeatherApp;

public record WeatherData(string City, int TemperatureC, string Country)
{
    public override string ToString() => $"{City}, {Country} {TemperatureFormat.Format(TemperatureC)}";
}
