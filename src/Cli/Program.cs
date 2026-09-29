using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

EnvironmentReport report = EnvironmentInfo.Collect();

/*if (args.Contains("--json"))
{
	Console.WriteLine(JsonSerializer.Serialize(report));
}
else
{
	Console.WriteLine("CrossApp – інформація про середовище");
	Console.WriteLine(new string('-', 52));

	Console.WriteLine($"ОС : {report.OsDescription}");
	Console.WriteLine($"Runtime : {report.FrameworkDescription}");
	Console.WriteLine($"Архітектура : {report.ProcessArchitecture}");
	Console.WriteLine($"RID (визначено): {report.DetectedRid}");
	Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
	Console.WriteLine($"Каталог : {report.BaseDirectory}");
	Console.WriteLine($"Примітка збірки : {report.BuildNote}");
}*/

string path = Path.GetFullPath(args.Length > 0
	? args[0]
	: Path.Combine(AppContext.BaseDirectory, "data", "sample.csv"));
if (!File.Exists(path))
	{
	Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
	return 1;
	}
object result = Path.GetExtension(path).ToLowerInvariant() switch
{
	".csv" => MixedCsvImporter.Load(path),
	".json" => ProductJsonImporter.Load(path),
	_ => new ImportResult<object>([], [$"Непідтримуване розширення файлу: {Path.GetExtension(path)}"])
};
switch (result)
	{
	case ImportResult<object> mixed:
		Console.WriteLine($"Завантажено записів: {mixed.Items.Count}");
		foreach (object item in mixed.Items)
		{
			switch (item)
			{
			case ProductDto product:
				Console.WriteLine($" Товар: {product.Id,-6} {product.Sku,-10} {product.Name,-26} {product.Quantity,5} {product.Unit}");
				break;
			case WarehouseDto warehouse:
				Console.WriteLine($" Склад: {warehouse.Id,-6} {warehouse.Name,-26} {warehouse.Address}");
				break;
			}
		}
		PrintErrors(mixed.Errors);
		break;
	case ImportResult<ProductDto> products:
		Console.WriteLine($"Завантажено записів: {products.Items.Count}");
		foreach (ProductDto product in products.Items)
			Console.WriteLine($" Товар: {product.Id,-6} {product.Sku,-10} {product.Name,-26} {product.Quantity,5} {product.Unit}");
		PrintErrors(products.Errors);
		break;
	}

static void PrintErrors(IReadOnlyList<string> errors)
	{
	if (errors.Count > 0)
	{
		Console.WriteLine($"Пропущено рядків: {errors.Count}");
		foreach (string error in errors)
			Console.WriteLine($" ! {error}");
	}
}
return 0;