using Core.Domain;

namespace Cli;

public static class DeferredExecutionDemo
{
    public static void Run(IEnumerable<Product> items)
    {
        // Локальний список: service.All() щоразу повертає новий знімок, тому зміни в ньому не побачили б.
        var source = items.ToList();

        var query = source.Where(p => p.Quantity < 10);                  // запит побудовано, не виконано
        Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}");    // число інше

        var snapshot = query.ToList();                                   // результат зафіксовано
        source.Clear();
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}");       // не змінюється
    }
}
