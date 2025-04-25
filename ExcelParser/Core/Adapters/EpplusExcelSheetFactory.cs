using ExcelParser.Core.Abstractions;
using OfficeOpenXml;

namespace ExcelParser.Core.Adapters;

public sealed class EpplusExcelSheetFactory : IExcelSheetFactory
{
    public async Task<IExcelSheet> CreateFromStreamAsync(Stream stream, string sheetName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var package = new ExcelPackage(stream); // ❗ не using
        var worksheet = package.Workbook.Worksheets[sheetName]
                        ?? throw new ArgumentException($"Лист '{sheetName}' не найден в файле.");

        var sheet = new EpplusExcelSheet(worksheet);

        return new EpplusExcelSheetWithPackage(sheet, package); // Оборачиваем в обёртку
    }
}
