using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Attributes;
using ExcelParser.Core.Parsers.Helpers;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Strategies;

/// <summary>
/// Стратегия простого последовательного парсинга Excel без распараллеливания.
/// Используется для маленьких файлов.
/// </summary>
public sealed class SimpleParseStrategy : IExcelParseStrategy
{
    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        var result = new List<TModel>();

        var headerMap = MappingHeader.Map<TModel>(sheet);
        if (!headerMap.Any()) return [];
        var properties = typeof(TModel).GetProperties();

        var modelType = typeof(TModel);

        // Маппинг заголовков
        for (int col = sheet.StartColumn; col <= sheet.EndColumn; col++)
        {
            var header = NormalizeHeader.Normalize(sheet.GetCellValue(sheet.StartRow, col));
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var property = ExcelPropertyMatcher.FindMatchingProperty(modelType, properties, header);
            if (property != null)
            {
                headerMap[col] = property;
            }
        }

        // Чтение данных строк
        for (int row = sheet.StartRow + 1; row <= sheet.EndRow; row++)
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

            result.Add(model);
        }

        return result;
    }

    
}
