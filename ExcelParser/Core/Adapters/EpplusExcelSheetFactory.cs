using ExcelParser.Core.Abstractions;
using OfficeOpenXml;

namespace ExcelParser.Core.Adapters;

public sealed class EpplusExcelSheetFactory : IExcelSheetFactory
{
    public async Task<IExcelSheet> CreateFromStreamAsync(Stream stream, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(stream);

        var worksheet = package.Workbook.Worksheets[sheetName]
                        ?? throw new ArgumentException($"Лист '{sheetName}' не найден в файле.");

        return new EpplusExcelSheet(worksheet);
    }
}
