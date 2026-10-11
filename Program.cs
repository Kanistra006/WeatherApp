// Погода по списку городов через API wttr.in.
// Запуск: dotnet run [-- путь_к_файлу_с_городами]
// По умолчанию читается Cities.txt рядом с исполняемым файлом.

using System.Text;
using WeatherApp;

Console.OutputEncoding = Encoding.UTF8;

var path = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "Cities.txt");

// 1. Загружаем список городов в память, убирая повторы (без учёта регистра).
var cities = File.ReadAllLines(path)
    .Select(line => line.Trim())
    .Where(line => line.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

// 2–4. Получаем погоду для каждого уникального города и выводим её.
var results = new List<WeatherData>();
using (var service = new WeatherService())
{
    Console.WriteLine("Погода по городам:");
    foreach (var city in cities)
    {
        try
        {
            var weather = await service.GetWeatherAsync(city);
            results.Add(weather);
            Console.WriteLine(weather);
        }
        catch (Exception error)
        {
            Console.WriteLine($"{city}: ошибка — {error.Message}");
        }
    }
}

// 5. Группируем по странам и считаем статистику.
Console.WriteLine();
Console.WriteLine("Статистика по странам:");
foreach (var group in results.GroupBy(w => w.Country).OrderBy(g => g.Key))
{
    var count = group.Count();
    var cityWord = count == 1 ? "city" : "cities";
    var avg = group.Average(w => w.TemperatureC);
    var min = group.Min(w => w.TemperatureC);
    var max = group.Max(w => w.TemperatureC);

    Console.WriteLine(
        $"{group.Key} — {count} {cityWord}, " +
        $"avg: {TemperatureFormat.Format(avg)}, min: {TemperatureFormat.Format(min)}, max: {TemperatureFormat.Format(max)}");
}
