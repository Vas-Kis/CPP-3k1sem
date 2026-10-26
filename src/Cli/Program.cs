using Core.Domain;
using Core.Dto;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 5);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);
Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", " ", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));
Console.WriteLine();
Console.WriteLine("=== Сценарій 3: перетворення результату імпорту ===");
ImportResult<ProductDto> importResult = new(
    [
        new ProductDto("", "SKU-004", "Фарба", "шт", 12),
        new ProductDto("P-005", "SKU-005", "Плитка", "м2", 30)
    ],
    ["рядок 2: помилка формату"],
    [1, 3]);
ImportResult<Product> converted = Product.FromImportResult(importResult);
Console.WriteLine($"Створено сутностей: {converted.Items.Count}");
foreach (string error in converted.Errors)
    Console.WriteLine($" ! {error}");
Console.WriteLine();
Console.WriteLine("=== Сценарій 4: ліміт відкритих видач читача ===");
Reader reader = Reader.Create("R-001", "Котакбас");
BookCopy[] copies = Enumerable.Range(1, Reader.MaximumOpenIssues + 1)
    .Select(number => BookCopy.Create($"C-{number:D3}", $"Книга {number}"))
    .ToArray();
foreach (BookCopy copy in copies.Take(Reader.MaximumOpenIssues))
    reader.Issue(copy);
TryDo("шоста відкрита видача", () => reader.Issue(copies[^1]));
Console.WriteLine($"Відкритих видач після відмови: {reader.OpenIssueCount}");
reader.Return(copies[0]);
reader.Issue(copies[^1]);
Console.WriteLine($"Після повернення та нової видачі: {reader.OpenIssueCount}");
Console.WriteLine();
Console.WriteLine("=== Сценарій 5: переходи станів замовлення ===");
Order order = Order.Create("O-001");
Console.WriteLine($"Початковий стан: {order.Status}");
order.TransitionTo(OrderStatus.Confirmed);
Console.WriteLine($"Після підтвердження: {order.Status}");
TryDo("повторне підтвердження", () => order.TransitionTo(OrderStatus.Confirmed));
order.TransitionTo(OrderStatus.Cancelled);
Console.WriteLine($"Після скасування: {order.Status}");
TryDo("перехід зі скасованого замовлення", () => order.TransitionTo(OrderStatus.Confirmed));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception exception)
    {
        Console.WriteLine($" {title}: {exception.GetType().Name} — {exception.Message}");
    }
}
