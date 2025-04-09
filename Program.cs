using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Словарь для хранения данных
Dictionary<string, List<string>> adPlatforms = new();


// Метод загрузки
app.MapPost("/upload", async ([FromForm] IFormFile file) =>
{
    adPlatforms.Clear(); // Очищаем словарь, перед загрузкой новых данных

    try
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Заполняем словарь
        foreach (var line in lines)
        {
            string[] parts = line.Split(':');

            if (parts.Length != 2) continue; // Строка задана некорректно. Пропускаем

            string platform = parts[0].Trim();
            var locations = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));

            foreach (var loc in locations)
            {
                if (!adPlatforms.ContainsKey(loc)) adPlatforms[loc] = new List<string>();
                adPlatforms[loc].Add(platform);
            }
        }
        return Results.Ok($"Данные успешно загружены. Количество загруженных локаций: {adPlatforms.Keys.Count}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Ошибка при загрузке данных: {ex.Message}");
    }
});

// Метод поиска
app.MapGet("/search", ([FromQuery] string locations) =>
{
    if (string.IsNullOrEmpty(locations)) return Results.BadRequest("Локация не указана");

    HashSet<string> result = new();

    foreach (var item in adPlatforms)
    {
        if (locations.StartsWith(item.Key)) result.UnionWith(item.Value);
    }

    return Results.Ok(result.ToList());
});

app.Run();