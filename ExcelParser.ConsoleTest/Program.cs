// See https://aka.ms/new-console-template for more information
using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Adapters;
using ExcelParser.Core.Attributes;
using ExcelParser.Core.Parsers;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Hello, World!");
// 1. Настройка DI
var services = new ServiceCollection();

services.AddScoped<IExcelSheetFactory, EpplusExcelSheetFactory>();
services.AddScoped<IExcelParserService, ExcelParserService>();

var provider = services.BuildServiceProvider();

// 2. Получаем сервис
var parserService = provider.GetRequiredService<IExcelParserService>();

// 3. Открываем файл
string path = @"D:\Test\test_large.xlsx";
using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

// 4. Читаем лист "Сотрудники"
var employees = await parserService.ParseFromStreamAsync<Employee>(stream, "Сотрудники");

// 5. Выводим результат красивой таблицей
Console.WriteLine("╔════════╦════════════════════╦════════╗");
Console.WriteLine("║  Код   ║        Имя         ║ Возраст║");
Console.WriteLine("╠════════╬════════════════════╬════════╣");

foreach (var employee in employees)
{
    Console.WriteLine($"║ {employee.Code.PadRight(6)} ║ {employee.Name.PadRight(18)} ║ {employee.Age.ToString().PadLeft(6)} ║");
}

Console.WriteLine("╚════════╩════════════════════╩════════╝");

Console.WriteLine("\nНажмите любую клавишу для выхода...");
Console.ReadKey();


/// <summary>
/// Модель сотрудника для парсинга из Excel.
/// </summary>
public class Employee
{
    [ExcelColumn("Код")]
    public string Code { get; set; } = default!;

    [ExcelColumn("Имя")]
    public string Name { get; set; } = default!;

    [ExcelColumn("Возраст")]
    public int Age { get; set; }
}