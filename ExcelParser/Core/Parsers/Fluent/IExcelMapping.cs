using System.Reflection;

namespace ExcelParser.Core.Parsers.Fluent;

/// <summary>
/// Описывает маппинг модели для парсинга Excel.
/// </summary>
public interface IExcelMapping
{
    string? GetColumnName(PropertyInfo property);
}