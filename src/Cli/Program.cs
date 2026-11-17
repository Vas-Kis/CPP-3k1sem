using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

//Definition of done 2
//Definition of done 4
bool useFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());
var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

try
{
    //Definition of done 5
    Product created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    service.Receive(created.Id, 25);
    service.Issue(created.Id, 10);

    Console.WriteLine($"Додано товар: {created.Id}; після приходу та видачі: {created.Quantity} {created.Unit}");

    Product? found = service.Find(created.Id);
    Console.WriteLine(found is null ? "Товар не знайдено." : $"Знайдено: {found}");

    Console.WriteLine("Каталог:");
    foreach (Product product in service.All())
        Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

    TryDo("отримання неіснуючого товару", () => service.Receive("missing-product", 1));
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Помилка аргументу: {exception.Message}");
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Помилка операції: {exception.Message}");
}

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: помилка НЕ спрацювала.");
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine($" {title}: {exception.Message}");
    }
}
