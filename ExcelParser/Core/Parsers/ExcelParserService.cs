using ExcelParser.Core.Abstractions;

namespace ExcelParser.Core.Parsers;

/// <summary>реализация сервиса парсинга Excel</summary>
public class ExcelParserService : IExcelParserService
{
    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        return await ExcelParserContext.ParseAsync<TModel>(sheet);
    }
}
