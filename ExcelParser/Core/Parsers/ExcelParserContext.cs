using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Parsers.Strategies;


namespace ExcelParser.Core.Parsers;

/// <summary>
/// Контекст парсера Excel, автоматически выбирающий стратегию в зависимости от количества строк.
/// </summary>
public static class ExcelParserContext
{
    public static async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        if (sheet.StartRow == 0 || sheet.EndRow == 0)
            return [];

        int rowsCount = sheet.EndRow - sheet.StartRow;

        IExcelParseStrategy strategy;

        if (rowsCount < 1000)
        {
            strategy = new SimpleParseStrategy();
        }
        else if (rowsCount < 10000)
        {
            strategy = new ParallelParseStrategy();
        }
        else
        {
            strategy = new DataflowParseStrategy();
        }

        return await strategy.ParseAsync<TModel>(sheet);
    }
}


