using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
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
}

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");
if (!File.Exists(path))
{
Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
return 1;
}
ImportResult<ProductDto> result = ProductCsvImporter.Load(path);
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
if (result.Errors.Count > 0)
{
Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
foreach (string e in result.Errors)
Console.WriteLine($" ! {e}");
}
return 0;