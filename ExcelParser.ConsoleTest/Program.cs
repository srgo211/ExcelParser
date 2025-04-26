// See https://aka.ms/new-console-template for more information
using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Adapters.Epplus;
using ExcelParser.Core.Adapters.ExcelDataReader;
using ExcelParser.Core.Attributes;
using ExcelParser.Core.Parsers;
using ExcelParser.Core.Parsers.Fluent;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using ExcelParser.Core.Parsers.Flexible;
using System.IO;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Hello, World!");
// 1. Настройка DI
var services = new ServiceCollection();

//services.AddScoped<IExcelSheetFactory, EpplusExcelSheetFactory>();
services.AddScoped<IExcelSheetFactory, ExcelDataReaderSheetFactory>();

services.AddScoped<IExcelParserService, ExcelParserService>();

var provider = services.BuildServiceProvider();

// 2. Получаем сервис
var parserService = provider.GetRequiredService<IExcelParserService>();



Stopwatch stopwatch = Stopwatch.StartNew();
//await Test1Async();
//await Test2Async();
await Test3Async();
stopwatch.Stop();
Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");

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

    ExcelMappingRegistry.Register<Contract>(config =>
    {
        config.Map(c => c.ContractNumber, "Номер договора");
        config.Map(c => c.Partner, "Контрагент");
        config.Map(c => c.SigningDate, "Дата подписания");
    });


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

async Task Test3Async()
{
    // 3. Конфигурация маппинга колонок
    var mappings = new ModelColumnsMappingBuilder()
        .For<Employee>(map => map
            .Map(1, x => x.Code)
            .Map(2, x => x.Name)
            .Map(3, x => x.Age)
            .StartWhen("Код")) // Старт при "Код"

        .For<Contract>(map => map
            .Map(5, x => x.ContractNumber)
            .Map(6, x => x.Partner)
            .Map(7, x => x.SigningDate)
            .StartWhen("Номер договора")) // Старт при "Номер договора"

        .For<Product>(map => map
            .Map(3, x => x.SKU)
            .Map(4, x => x.Name)
            .Map(5, x => x.Price)
            .StartWhen("Артикул")) // Старт при "Артикул"
        .Build();

    

    var sheetFactory = provider.GetRequiredService<IExcelSheetFactory>();

    string path=@"D:\Test\test_flexible.xlsx";
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    var sheet = await sheetFactory.CreateFromStreamAsync(stream, "Лист1");

    var parsed = await parserService.ParseFlexibleByColumnsAsync(sheet, mappings);

    foreach (var (modelType, models) in parsed)
    {
        Console.WriteLine($"\n=== Модель: {modelType.Name} ===");
        Console.WriteLine($"Количество записей: {models.Count}\n");

        if (models.Count == 0)
        {
            Console.WriteLine("Нет данных.\n");
            continue;
        }

        var firstItem = models[0];
        var properties = firstItem.GetType().GetProperties();

        Console.WriteLine(string.Join(" | ", properties.Select(p => p.Name.PadRight(20))));
        Console.WriteLine(new string('-', properties.Length * 23));

        foreach (var item in models)
        {
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item)?.ToString() ?? "";
                Console.Write(value.PadRight(20) + " | ");
            }
            Console.WriteLine();
        }
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
    public string ContractNumber { get; set; } = default!;   
    public string Partner { get; set; } = default!;    
    public DateTime SigningDate { get; set; }
}

#endregion