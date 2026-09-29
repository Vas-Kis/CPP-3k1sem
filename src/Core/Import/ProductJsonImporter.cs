using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<object> Load(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        using JsonDocument document = JsonDocument.Parse(json);
        var items = new List<object>();
        var errors = new List<string>();

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            string? type = element.TryGetProperty("type", out JsonElement typeElement)
                ? typeElement.GetString()?.ToUpperInvariant()
                : null;

            switch (type)
            {
                case "P":
                    ProductDto? product = JsonSerializer.Deserialize<ProductDto>(element, options);
                    if (product is not null)
                        items.Add(product);
                    break;
                case "W":
                    WarehouseDto? warehouse = JsonSerializer.Deserialize<WarehouseDto>(element, options);
                    if (warehouse is not null)
                        items.Add(warehouse);
                    break;
                default:
                    errors.Add("Об'єкт JSON має невідомий або відсутній type");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }
}