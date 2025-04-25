namespace ExcelParser.Core.Abstractions;

public interface IExcelSheetFactory
{
    Task<IExcelSheet> CreateFromStreamAsync(Stream stream, string sheetName);
}
