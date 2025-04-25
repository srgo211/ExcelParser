namespace ExcelParser.Core.Abstractions;

public interface IExcelSheetFactory
{
   
    Task<IExcelSheet> CreateFromStreamAsync(Stream stream, string sheetName);

    /// <summary>
    /// Создаёт список всех листов из потока Excel-файла.
    /// </summary>
    /// <param name="stream">Поток с Excel-данными.</param>
    /// <returns>Список пар (Имя листа, Объект листа).</returns>
    Task<List<(string SheetName, IExcelSheet Sheet)>> CreateAllSheetsFromStreamAsync(Stream stream);
}
