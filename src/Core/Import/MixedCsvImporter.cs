using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith("type;", StringComparison.OrdinalIgnoreCase))
                continue;

            string[] parts = line.Split(';', StringSplitOptions.TrimEntries);
            switch (parts.FirstOrDefault())
            {
                case "P" when parts.Length == 6 && !string.IsNullOrWhiteSpace(parts[2]) && !string.IsNullOrWhiteSpace(parts[3]) && int.TryParse(parts[5], out int quantity) && quantity >= 0:
                    items.Add(new ProductDto(parts[1], parts[2], parts[3], parts[4], quantity));
                    break;
                case "W" when parts.Length == 4 && !string.IsNullOrWhiteSpace(parts[1]) && !string.IsNullOrWhiteSpace(parts[2]):
                    items.Add(new WarehouseDto(parts[1], parts[2], parts[3]));
                    break;
                case "P":
                    errors.Add($"рядок {i + 1}: некоректний товарний запис");
                    break;
                case "W":
                    errors.Add($"рядок {i + 1}: некоректний запис складу");
                    break;
                default:
                    errors.Add($"рядок {i + 1}: невідомий префікс типу");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }
}