using ExcelDataReader;
using ExcelParser.Core.Abstractions;
using System.Data;

namespace ExcelParser.Core.Adapters.ExcelDataReader;

/// <summary>
/// Фабрика для создания листов Excel через ExcelDataReader.
/// </summary>
public sealed class ExcelDataReaderSheetFactory : IExcelSheetFactory
{
    public async Task<IExcelSheet> CreateFromStreamAsync(Stream stream, string sheetName)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet();

        var table = result.Tables.Cast<DataTable>()
                        .FirstOrDefault(t => t.TableName.Equals(sheetName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Лист '{sheetName}' не найден в файле.");

        return new ExcelDataReaderExcelSheet(table);
    }

    public async Task<List<(string SheetName, IExcelSheet Sheet)>> CreateAllSheetsFromStreamAsync(Stream stream)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var result = reader.AsDataSet();

        var sheets = result.Tables.Cast<DataTable>()
            .Select(t => (t.TableName, (IExcelSheet)new ExcelDataReaderExcelSheet(t)))
            .ToList();

        return sheets;
    }
}