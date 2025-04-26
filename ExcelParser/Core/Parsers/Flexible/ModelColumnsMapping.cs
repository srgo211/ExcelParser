using System.Reflection;

namespace ExcelParser.Core.Parsers.Flexible;

/// <summary>
/// Маппинг модели и её колонок.
/// </summary>
public class ModelColumnsMapping
{
    public Type ModelType { get; }
    public Dictionary<int, PropertyInfo> ColumnPropertyMap { get; }
    public string? StartKeyword { get; }

    /// <summary>пропустить первую строку заголовков</summary>
    public int SkipRows { get; }

    public ModelColumnsMapping(Type modelType, Dictionary<int, PropertyInfo> columnPropertyMap, string? startKeyword = null, int skipRows = 0)
    {
        ModelType = modelType;
        ColumnPropertyMap = columnPropertyMap;
        StartKeyword = startKeyword;
        SkipRows = skipRows;
    }
}