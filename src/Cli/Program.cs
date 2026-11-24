using Cli;
using Core.Abstractions;
using Core.Reports;
using Core.Services;

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

// Прапорці (--file, --cache) не є назвою команди.
// lab07 Step 4 - перший аргумент без "--" є назвою звіту; якщо його немає, береться report-top.
var command = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)) ?? "report-top";

// Демо подій саме змінює каталог, тому виконується до отримання знімка для звітів.
if (command == "demo-events")
{
    EventsDemo.Run(store, service);
    return 0;
}

var items = service.All();
// lab07 Step 4, DoD 5 - чотири підкоманди звітів; тут лише виклики ReportService і вивід.
switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;
    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;
    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($"  {g.Key,-8} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,6}");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"  позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;
    case "demo-deferred":
        // lab07 Step 3, DoD 6 - запуск демо відкладеного виконання.
        DeferredExecutionDemo.Run(items);
        break;
    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum | demo-deferred | demo-events");
        break;
}

return 0;

// lab07 Step 4, DoD 5 - вирівнювання стовпців: від'ємна ширина - ліворуч, додатна - праворуч.
static void Print(IReadOnlyList<StockLine> lines)
{
    if (lines.Count == 0)
        Console.WriteLine("  (немає)");
    foreach (var l in lines)
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}
