using Core.Domain;

namespace Cli;

// lab07 Step 3, DoD 6 - демонстрація відкладеного виконання (пояснення також є в README).
// lab07 Step 6 - Where лише описує запит; він виконується під час кожного виклику Count() і під час ToList().
// Перший Count() дав 3, а другий - 4, бо між ними до source додано SKU-999 із залишком 3,
// і другий виклик заново перебрав уже змінене джерело. ToList() зафіксував результат в окремому списку,
// тому source.Clear() на snapshot не впливає.
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
