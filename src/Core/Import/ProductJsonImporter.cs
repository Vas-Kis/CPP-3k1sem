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
        var items = new List<object>();
        var errors = new List<string>();
        List<(int Number, string Json)> entries = ExtractEntries(json, errors);

        foreach ((int number, string entry) in entries)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(entry);
                JsonElement element = document.RootElement;
                if (element.ValueKind != JsonValueKind.Object)
                {
                    errors.Add($"об'єкт JSON {number}: очікую об'єкт");
                    continue;
                }

                string? type = element.TryGetProperty("type", out JsonElement typeElement)
                    && typeElement.ValueKind == JsonValueKind.String
                    ? typeElement.GetString()?.ToUpperInvariant()
                    : null;

                switch (type)
                {
                    case "P":
                        ProductDto? product = JsonSerializer.Deserialize<ProductDto>(element, options);
                        if (product is null)
                            errors.Add($"об'єкт JSON {number}: товар не може бути null");
                        else if (string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.Name))
                            errors.Add($"об'єкт JSON {number}: SKU або назва порожні");
                        else if (product.Quantity < 0)
                            errors.Add($"об'єкт JSON {number}: кількість не може бути від'ємною");
                        else
                            items.Add(product);
                        break;
                    case "W":
                        WarehouseDto? warehouse = JsonSerializer.Deserialize<WarehouseDto>(element, options);
                        if (warehouse is null)
                            errors.Add($"об'єкт JSON {number}: склад не може бути null");
                        else if (string.IsNullOrWhiteSpace(warehouse.Id) || string.IsNullOrWhiteSpace(warehouse.Name))
                            errors.Add($"об'єкт JSON {number}: некоректний запис складу");
                        else
                            items.Add(warehouse);
                        break;
                    default:
                        errors.Add($"об'єкт JSON {number}: невідомий або відсутній type");
                        break;
                }
            }
            catch (JsonException exception)
            {
                errors.Add($"об'єкт JSON {number}: {exception.Message}");
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static List<(int Number, string Json)> ExtractEntries(string json, List<string> errors)
    {
        var entries = new List<(int Number, string Json)>();
        int index = 0;
        SkipWhitespace(json, ref index);
        if (index == json.Length || json[index] != '[')
        {
            errors.Add("кореневий елемент JSON має бути масивом");
            return entries;
        }

        index++;
        int number = 0;
        bool needsSeparator = false;
        while (true)
        {
            SkipWhitespace(json, ref index);
            if (index == json.Length)
            {
                errors.Add("некоректний JSON: відсутня закривальна дужка масиву");
                break;
            }

            if (json[index] == ']')
            {
                index++;
                SkipWhitespace(json, ref index);
                if (index < json.Length)
                    errors.Add("некоректний JSON: зайві дані після масиву");
                break;
            }

            if (needsSeparator)
            {
                if (json[index] == ',')
                {
                    index++;
                    SkipWhitespace(json, ref index);
                    if (index < json.Length && json[index] == ']')
                    {
                        errors.Add("некоректний JSON: зайва кома перед закривальною дужкою масиву");
                        break;
                    }
                }
                else
                {
                    errors.Add($"некоректний JSON після об'єкта {number}: очікую кому між записами");
                }
            }
            else if (json[index] == ',')
            {
                number++;
                errors.Add($"об'єкт JSON {number}: зайва кома між записами");
                index++;
                continue;
            }

            number++;
            int start = index;
            if (json[index] == '{')
            {
                int depth = 0;
                bool inString = false;
                bool escaped = false;
                do
                {
                    char current = json[index++];
                    if (inString)
                    {
                        if (escaped)
                            escaped = false;
                        else if (current == '\\')
                            escaped = true;
                        else if (current == '"')
                            inString = false;
                    }
                    else if (current == '"')
                    {
                        inString = true;
                    }
                    else if (current == '{')
                    {
                        depth++;
                    }
                    else if (current == '}')
                    {
                        depth--;
                    }
                }
                while (index < json.Length && depth > 0);

                entries.Add((number, json[start..index]));
                if (depth > 0)
                {
                    errors.Add($"об'єкт JSON {number}: некоректний JSON, об'єкт не закритий");
                    break;
                }
            }
            else
            {
                while (index < json.Length && json[index] != ',' && json[index] != ']')
                    index++;
                entries.Add((number, json[start..index]));
            }

            needsSeparator = true;
        }

        return entries;
    }

    private static void SkipWhitespace(string value, ref int index)
    {
        while (index < value.Length && char.IsWhiteSpace(value[index]))
            index++;
    }
}