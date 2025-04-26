using ExcelParser.Core.Parsers.Helpers;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Flexible;

/// <summary>
/// Гибкий парсер строки в модель по колонкам.
/// </summary>
public static class FlexibleRowMapper
{
    public static object? MapRowToModel(Type modelType, Dictionary<int, PropertyInfo> columnToProperty, Func<int, string> getCellValue)
    {
        var model = Activator.CreateInstance(modelType)!;
        bool hasData = false;

        foreach (var (col, prop) in columnToProperty)
        {
            var value = getCellValue(col);
            if (string.IsNullOrWhiteSpace(value))
                continue;

            try
            {
                object? converted = ConvertHelper.ConvertTo(value, prop.PropertyType);
                prop.SetValue(model, converted);
                hasData = true;
            }
            catch
            {
                // Ошибку можно логировать
            }
        }

        return hasData ? model : null;
    }

}