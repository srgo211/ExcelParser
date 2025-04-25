using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Attributes;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using ExcelParser.Core.Parsers.Helpers;

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

        var startRow = sheet.StartRow;
        var endRow = sheet.EndRow;
        var startCol = sheet.StartColumn;
        var endCol = sheet.EndColumn;

        if (startRow == 0 || endRow == 0 || startCol == 0 || endCol == 0)
            return [];

        var headerMap = new Dictionary<int, PropertyInfo>();
        var properties = typeof(TModel).GetProperties();

        // Маппинг заголовков
        for (int col = startCol; col <= endCol; col++)
        {
            var header = sheet.GetCellValue(startRow, col).Trim();
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var property = properties.FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ExcelColumnAttribute>();
                return attr != null ? attr.ColumnName.Equals(header, StringComparison.OrdinalIgnoreCase) : p.Name.Equals(header, StringComparison.OrdinalIgnoreCase);
            });

            if (property != null)
            {
                headerMap[col] = property;
            }
        }

        // Параллельное чтение строк
        Parallel.For(startRow + 1, endRow + 1, row =>
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
                    // Можно добавить логирование ошибок
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

