using System.Reflection;

namespace ExcelParser.Core.Parsers.Fluent;

/// <summary>Конкретный маппинг модели через Fluent API.</summary>
public class ExcelMapping<T> : IExcelMapping
{
    private readonly Dictionary<string, string> _propertyToColumn = new();

    public void Map(PropertyInfo property, string columnName)
    {
        _propertyToColumn[property.Name] = columnName;
    }

    public string? GetColumnName(PropertyInfo property)
    {
        return _propertyToColumn.TryGetValue(property.Name, out var columnName) ? columnName : null;
    }
}