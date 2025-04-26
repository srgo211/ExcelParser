using ExcelParser.Core.Parsers.Flexible;
using System.Collections;

namespace ExcelParser.Core.Abstractions;

/// <summary>Сервис для универсального парсинга Excel-листов в модели.</summary>
public interface IExcelParserService
{
    /// <summary>Парсит Excel-лист в список моделей</summary>
    Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new();

    /// <summary>
    /// Парсит Excel-файл из потока в список моделей.
    /// </summary>
    Task<List<TModel>> ParseFromStreamAsync<TModel>(Stream stream, string sheetName) where TModel : new();


    /// <summary>Парсит несколько листов в разные модели согласно маппингу. </summary>
    /// <param name="stream">Поток Excel-файла.</param>
    /// <param name="sheetMappings">Маппинг: название листа → тип модели.</param>
    /// <returns>Словарь: название листа → список объектов моделей.</returns>
    Task<Dictionary<string, IList>> ParseSheetsWithMappingAsync(Stream stream, Dictionary<string, Type> sheetMappings);


    /// <summary>Гибкий парсинг разных моделей с одного листа по колонкам</summary>
    Task<Dictionary<Type, IList>> ParseFlexibleByColumnsAsync(IExcelSheet sheet, List<ModelColumnsMapping> mappings);
}
