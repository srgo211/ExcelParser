using ExcelParser.Core.Attributes;
using ExcelParser.Core.Parsers.Fluent;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Helpers;

/// <summary>
/// Помощник для поиска соответствий между колонками Excel и свойствами моделей.
/// </summary>
public static class ExcelPropertyMatcher
{
    public static PropertyInfo? FindMatchingProperty(Type modelType, PropertyInfo[] properties, string header)
    {
        var mapping = ExcelMappingRegistry.GetMapping(modelType);

        foreach (var prop in properties)
        {
            // 1. Проверка через Fluent
            if (mapping != null)
            {
                var fluentName = mapping.GetColumnName(prop);
                if (fluentName != null && fluentName.Equals(header, StringComparison.OrdinalIgnoreCase))
                    return prop;
            }

            // 2. Проверка через атрибут
            var attr = prop.GetCustomAttribute<ExcelColumnAttribute>();
            if (attr != null && attr.ColumnName.Equals(header, StringComparison.OrdinalIgnoreCase))
                return prop;

            // 3. Проверка через имя свойства
            if (prop.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                return prop;
        }

        return null;
    }
}