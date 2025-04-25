using System.Collections;
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

    public async Task<Dictionary<string, IList>> ParseSheetsWithMappingAsync(Stream stream, Dictionary<string, Type> sheetMappings)
    {
        var sheets = await _sheetFactory.CreateAllSheetsFromStreamAsync(stream);

        var result = new Dictionary<string, IList>();

        foreach (var (sheetName, modelType) in sheetMappings)
        {
            var sheet = sheets.FirstOrDefault(s => s.SheetName.Equals(sheetName, StringComparison.OrdinalIgnoreCase)).Sheet;
            if (sheet == null)
                continue;

            var parsedList = await ParseByTypeAsync(sheet, modelType);
            result.Add(sheetName, parsedList);
        }

        return result;
    }

    private async Task<IList> ParseByTypeAsync(IExcelSheet sheet, Type modelType)
    {
        var method = typeof(ExcelParserContext)
            .GetMethod(nameof(ExcelParserContext.ParseAsync))!
            .MakeGenericMethod(modelType);

        var task = (Task)method.Invoke(null, new object[] { sheet })!;
        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result")!;
        return (IList)resultProperty.GetValue(task)!;
    }
}
