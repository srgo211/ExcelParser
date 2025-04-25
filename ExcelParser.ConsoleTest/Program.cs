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




//await Test1Async();
await Test2Async();

Console.WriteLine("\nНажмите любую клавишу для выхода...");
Console.ReadKey();



async Task Test1Async()
{
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

}


async Task Test2Async()
{
    var sheetMappings = new Dictionary<string, Type>
    {
        ["Сотрудники"] = typeof(Employee),
        ["Товары"]     = typeof(Product),
        ["Контракты"]  = typeof(Contract)
    };
    string path = @"D:\Test\test_multisheet.xlsx";
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

    // 5. Парсинг всех листов
    var parsedSheets = await parserService.ParseSheetsWithMappingAsync(stream, sheetMappings);

    // 6. Красивый вывод
    foreach (var (sheetName, models) in parsedSheets)
    {
        Console.WriteLine($"\n=== Лист: {sheetName} ===");
        Console.WriteLine($"Количество записей: {models.Count}\n");

        if (models.Count == 0)
        {
            Console.WriteLine("Нет данных.\n");
            continue;
        }

        var firstItem = models[0];
        var properties = firstItem.GetType().GetProperties();

        // Заголовок таблицы
        Console.WriteLine(string.Join(" | ", properties.Select(p => p.Name.PadRight(20))));
        Console.WriteLine(new string('-', properties.Length * 23));

        // Строки данных
        foreach (var item in models)
        {
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item)?.ToString() ?? "";
                Console.Write(value.PadRight(20) + " | ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

}

#region Models

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

public class Product
{
    [ExcelColumn("Артикул")]
    public string SKU { get; set; } = default!;

    [ExcelColumn("Название товара")]
    public string Name { get; set; } = default!;

    [ExcelColumn("Цена")]
    public decimal Price { get; set; }
}

public class Contract
{
    [ExcelColumn("Номер договора")]
    public string ContractNumber { get; set; } = default!;

    [ExcelColumn("Контрагент")]
    public string Partner { get; set; } = default!;

    [ExcelColumn("Дата подписания")]
    public DateTime SigningDate { get; set; }
}

#endregion