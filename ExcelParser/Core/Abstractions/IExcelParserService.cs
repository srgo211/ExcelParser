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

}
