using System.Text.Json;
using Core;

var environment = EnvironmentInfo.Collect();

var data = new
{
	OSDescription = environment.OsDescription,
	EnvironmentOS = environment.EnvironmentOS,
	ProcessArchitecture = environment.ProcessArchitecture,
	DotNetVersion = environment.DotNetVersion,
	Runtime = environment.FrameworkDescription,
	AppDirectory = environment.BaseDirectory,
	CurrentDirectory = environment.CurrentDirectory,
	SubjectArea = "Склад (товари, партії, залишки, переміщення)"
};

if (args.Contains("--json"))
{
	Console.WriteLine(JsonSerializer.Serialize(data));
}
else
{
	Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
	Console.WriteLine("Студент: Кислянка Василь, група ФеІ-32");
	Console.WriteLine(new string('-', 52));

	Console.WriteLine($"ОС (OSDescription) : {data.OSDescription}");
	Console.WriteLine($"ОС (Environment) : {data.EnvironmentOS}");
	Console.WriteLine($"Архітектура процесу : {data.ProcessArchitecture}");
	Console.WriteLine($"Версія .NET (CLR) : {data.DotNetVersion}");
	Console.WriteLine($"Runtime : {data.Runtime}");
	Console.WriteLine($"Каталог застосунку : {data.AppDirectory}");
	Console.WriteLine($"Поточний каталог : {data.CurrentDirectory}");

	Console.WriteLine(new string('-', 52));
	Console.WriteLine($"Предметна область: {data.SubjectArea}");
}
