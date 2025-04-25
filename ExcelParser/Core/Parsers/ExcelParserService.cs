using ExcelParser.Core.Abstractions;

namespace ExcelParser.Core.Parsers;

/// <summary>реализация сервиса парсинга Excel</summary>
public class ExcelParserService : IExcelParserService
{
    private readonly IExcelSheetFactory _sheetFactory;

    public ExcelParserService(IExcelSheetFactory sheetFactory)
    {
        _sheetFactory = sheetFactory;
    }

    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        return await ExcelParserContext.ParseAsync<TModel>(sheet);
    }

    public async Task<List<TModel>> ParseFromStreamAsync<TModel>(Stream stream, string sheetName) where TModel : new()
    {
        var sheet = await _sheetFactory.CreateFromStreamAsync(stream, sheetName);
        return await ParseAsync<TModel>(sheet);
    }
}
