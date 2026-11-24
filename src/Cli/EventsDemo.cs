using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

namespace Cli;

public static class EventsDemo
{
    public static void Run(ICatalogStore store, CatalogService service)
    {
        Console.WriteLine($"Сховище: {store.GetType().Name}");

        string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
        {
            string line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
            Console.WriteLine(line);

            try
            {
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch (IOException exception)
            {
                Console.WriteLine($" Не вдалося записати лог: {exception.Message}");
            }
        }

        service.Changed += OnCatalogChanged;

        int changes = 0;
        service.Changed += (_, _) => changes++;

        try
        {
            Product created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
            service.Receive(created.Id, 25);
            service.Issue(created.Id, 10);
            service.Add("SKU-102", "Розетка подвійна", "шт", 8);
            Console.WriteLine($"Подій отримано: {changes}");

            TryDo("прихід з від'ємною кількістю", () => service.Receive(created.Id, -5));
            TryDo("прихід для неіснуючого товару", () => service.Receive("missing-product", 1));
            Console.WriteLine($"Подій отримано після відмов: {changes}");

            service.Changed -= OnCatalogChanged;
            service.Receive(created.Id, 5);
            Console.WriteLine($"Подій отримано: {changes}; залишок {created.Id}: {created.Quantity} {created.Unit}");

            Product? found = service.Find(created.Id);
            Console.WriteLine(found is null ? "Товар не знайдено." : $"Знайдено: {found}");

            Console.WriteLine("Каталог:");
            foreach (Product product in service.All())
                Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

            Console.WriteLine("Пошук: товари з залишком менше 10:");
            IReadOnlyList<Product> lowStock = service.FindAll(p => p.Quantity < 10);
            if (lowStock.Count == 0)
                Console.WriteLine("  (немає)");
            foreach (Product product in lowStock)
                Console.WriteLine($"  {product.Id}  {product.Sku,-10} {product.Name,-24} {product.Quantity,5} {product.Unit}");

            if (store is CachingCatalogStore caching)
            {
                service.All();
                service.Find(created.Id);
                Console.WriteLine($"Кеш: влучань {caching.Hits}, промахів {caching.Misses}");
            }
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Помилка аргументу: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            Console.WriteLine($"Помилка операції: {exception.Message}");
        }
    }

    private static void TryDo(string title, Action action)
    {
        try
        {
            action();
            Console.WriteLine($" {title}: помилка НЕ спрацювала.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            Console.WriteLine($" {title}: {exception.Message}");
        }
    }
}
