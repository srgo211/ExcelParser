using ExcelParser.Core.Abstractions;
using OfficeOpenXml;

namespace ExcelParser.Core.Adapters.Epplus;



/// <summary>
/// Адаптер для работы с листами EPPlus через абстракцию IExcelSheet.
/// Позволяет использовать EPPlus без привязки к конкретной реализации в парсере.
/// </summary>
public sealed class EpplusExcelSheet : IExcelSheet
{
    private readonly ExcelWorksheet _worksheet;

    public EpplusExcelSheet(ExcelWorksheet worksheet)
    {
        _worksheet = worksheet ?? throw new ArgumentNullException(nameof(worksheet));
    }

    public int StartRow => _worksheet?.Dimension?.Start.Row ?? 0;

    public int EndRow => _worksheet?.Dimension?.End.Row ?? 0;

    public int StartColumn => _worksheet?.Dimension?.Start.Column ?? 0;

    public int EndColumn => _worksheet?.Dimension?.End.Column ?? 0;

    public string GetCellValue(int row, int column)
    {
        return _worksheet.Cells[row, column]?.Text?.Trim() ?? string.Empty;
    }
}

