using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Attributes;
using System.Globalization;
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

        var startRow = sheet.StartRow;
        var endRow = sheet.EndRow;
        var startCol = sheet.StartColumn;
        var endCol = sheet.EndColumn;

        if (startRow == 0 || endRow == 0 || startCol == 0 || endCol == 0)
            return result;

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

        // Чтение данных
        for (int row = startRow + 1; row <= endRow; row++)
        {
            var model = new TModel();

            foreach (var (col, prop) in headerMap)
            {
                var cellValue = sheet.GetCellValue(row, col);

                if (string.IsNullOrWhiteSpace(cellValue))
                    continue;

                try
                {
                    object? converted = ConvertTo(cellValue, prop.PropertyType);
                    prop.SetValue(model, converted);
                }
                catch
                {
                    // Тут потом можно добавить логирование ошибок парсинга
                }
            }

            result.Add(model);
        }

        return result;
    }

    private static object? ConvertTo(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (string.IsNullOrWhiteSpace(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, value, ignoreCase: true);
        }

        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
        {
            //Пробуем через (. точка)
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInvariant))
                return Convert.ChangeType(resultInvariant, underlyingType);
            //Пробуем через (локальная настройка, может быть ,)
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var resultCurrent))
                return Convert.ChangeType(resultCurrent, underlyingType);
        }

        //Конвертируем стандартно через локаль
        return Convert.ChangeType(value, underlyingType, CultureInfo.CurrentCulture);
    }
}
