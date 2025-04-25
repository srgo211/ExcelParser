using System.Data;
using ExcelParser.Core.Abstractions;

namespace ExcelParser.Core.Adapters.ExcelDataReader;




/// <summary>
/// Адаптер для листа Excel, реализованный через ExcelDataReader.
/// Работает через DataTable.
/// </summary>
public sealed class ExcelDataReaderExcelSheet : IExcelSheet
{
    private readonly DataTable _table;

    public ExcelDataReaderExcelSheet(DataTable table)
    {
        _table = table ?? throw new ArgumentNullException(nameof(table));
    }

    public int StartRow => 1;
    public int EndRow => _table.Rows.Count;

    public int StartColumn => 1;
    public int EndColumn => _table.Columns.Count;

    public string GetCellValue(int row, int column)
    {
        if (row < 1 || column < 1)
            return string.Empty;

        if (row > _table.Rows.Count || column > _table.Columns.Count)
            return string.Empty;

        var value = _table.Rows[row - 1][column - 1];
        return value?.ToString()?.Trim() ?? string.Empty;
    }
}
