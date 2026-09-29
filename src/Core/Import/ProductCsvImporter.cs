using System.Text;
using Core.Dto;
namespace Core.Import;
public static class ProductCsvImporter
{
// Роздільник — крапка з комою: не конфліктує з комою в назвах товарів.
private const char Separator = ';';
public static ImportResult<ProductDto> Load(string path)
        {
        var items = new List<ProductDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++)
            {
            int number = i + 1;
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
            continue; // рядок заголовків
            switch (ParseLine(line))
                {
                case ParseOk ok:
                items.Add(ok.Value);
                break;
                case ParseFailed failed:
                errors.Add($"рядок {number}: {failed.Reason}");
                break;
                }
            }
        return new ImportResult<ProductDto>(items, errors);
        }
private static ParseOutcome ParseLine(string line)
/*Патерн — це синтаксична конструкція, яка перевіряє, чи відповідають дані певній формі, 
і дозволяє одночасно витягти ці дані у змінні. Дає змогу замінити довгий ланцюжок 
if-else компактною конструкцією, яка читається як таблиця правил.*/

    {
    string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);
    return parts switch
        {
        //спрацює, якщо в масиві менше ніж п'ять елементів
        { Length: < 5 } => new ParseFailed($"очікую 5 колонок, отримав {parts.Length}"),
        //спрацює, якщо друга (SKU) АБО третя (назва) колонка порожня
        [_, "", _, _, _] or [_, _, "", _, _]
        => new ParseFailed("SKU або назва порожні"), 
        //перевіряє чи рядок є числом (out int q) і невід'ємним (||q<0)
        [_, _, _, _, var qty] when !int.TryParse(qty, out int q) || q < 0
        => new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),
        //Якщо рядок дійшов до цієї гілки, він вважається коректним, і створюється об'єкт ProductDto
        [var id, var sku, var name, var unit, var qty]
        => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),
        //перевіряє варіанти, НЕописані вище (масив із > 5 об'єктами)
        _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }
private abstract record ParseOutcome;
private sealed record ParseOk(ProductDto Value) : ParseOutcome;
private sealed record ParseFailed(string Reason) : ParseOutcome;
}