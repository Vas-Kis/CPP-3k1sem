using Core.Domain;

namespace Core.Reports;

// lab07 Step 2 - чисті функції: приймають IEnumerable<Product>, а не сховище, тому їх легко викликати з будь-якого джерела.
// lab07 DoD 1 - чотири різні LINQ-сценарії в Core: фільтр, топ-N, групування, агрегати; проєкція Select у DTO.
// lab07 DoD 2 - звіти приймають IEnumerable<T> і повертають IReadOnlyList<DTO>;
// Totals повертає іменований кортеж (як у кроці 2), бо чотири числа не є рядком таблиці.
// lab07 DoD 7 - лише LINQ to Objects: без AsParallel і без запитів до бази даних.
public static class ReportService
{
    // lab07 Step 2, DoD 1 - Where + OrderBy/ThenBy + Select у DTO; фільтр стоїть до сортування, щоб не сортувати зайве.
    // lab07 DoD 4 - для порожнього джерела ToList поверне порожній список без винятку.
    // 1) фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // lab07 Step 2, DoD 1 - топ-N: сортування за спаданням залишку + Take.
    // lab07 DoD 3 - другий ключ сортування ThenBy(Sku): за однакового залишку порядок не залежить від порядку в джерелі.
    // 2) топ-N зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Take(n)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // lab07 Step 2, DoD 1 - GroupBy: g.Key - одиниця виміру, g.Count() - кількість позицій, g.Sum() - сумарний залишок групи.
    // OrdinalIgnoreCase об'єднує "шт" і "ШТ" в одну групу.
    // 3) групування з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
            .OrderByDescending(s => s.TotalQuantity)
            .ToList();

    // lab07 Step 2, DoD 1 - агрегати Sum, Average, Max; ToList один раз, щоб не перебирати джерело повторно.
    // lab07 DoD 4 - Average і Max на порожній послідовності кидають InvalidOperationException, тому спершу перевіряємо Count == 0.
    // 4) агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
               list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }
}
