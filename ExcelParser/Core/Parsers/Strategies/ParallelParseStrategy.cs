using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Parsers.Helpers;
using System.Collections.Concurrent;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Strategies;

/// <summary>
/// Стратегия параллельного парсинга Excel через Parallel.For.
/// Используется для файлов среднего размера.
/// </summary>
public sealed class ParallelParseStrategy : IExcelParseStrategy
{
    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        var result = new ConcurrentDictionary<int, TModel>();

        var headerMap = MappingHeader.Map<TModel>(sheet);
        if (!headerMap.Any()) return [];

        // Параллельное чтение строк
        Parallel.For(sheet.StartRow + 1, sheet.EndRow + 1, row =>
        {
            var model = new TModel();

            foreach (var (col, prop) in headerMap)
            {
                var cellValue = sheet.GetCellValue(row, col);

                if (string.IsNullOrWhiteSpace(cellValue))
                    continue;

                try
                {
                    object? converted = ConvertHelper.ConvertTo(cellValue, prop.PropertyType);
                    prop.SetValue(model, converted);
                }
                catch
                {
                    // Ошибки парсинга можно логировать
                }
            }

            result.TryAdd(row, model);
        });

        return result
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();
    }

    
}
