namespace ExcelParser.Core.Attributes;

/// <summary> Атрибут для указания, к какой колонке Excel привязано свойство модели. </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ExcelColumnAttribute : Attribute
{
    /// <summary>Название колонки Excel.</summary>
    public string ColumnName { get; }

    public ExcelColumnAttribute(string columnName)
    {
        ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
    }
}
