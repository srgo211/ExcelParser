using ExcelParser.Core.Abstractions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelParser.Core.Adapters.Epplus;

/// <summary>
/// Обёртка для работы с ExcelSheet и управлением временем жизни ExcelPackage.
/// </summary>
public sealed class EpplusExcelSheetWithPackage : IExcelSheet, IDisposable
{
    private readonly IExcelSheet _sheet;
    private readonly ExcelPackage _package;

    public EpplusExcelSheetWithPackage(IExcelSheet sheet, ExcelPackage package)
    {
        _sheet = sheet;
        _package = package;
    }

    public int StartRow => _sheet.StartRow;
    public int EndRow => _sheet.EndRow;
    public int StartColumn => _sheet.StartColumn;
    public int EndColumn => _sheet.EndColumn;

    public string GetCellValue(int row, int column) => _sheet.GetCellValue(row, column);

    public void Dispose()
    {
        _package.Dispose();
    }
}
