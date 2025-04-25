namespace ExcelParser.Core.Abstractions;

/// <summary>Абстракция для представления листа Excel, независимого от конкретной библиотеки (EPPlus, ClosedXML и т.д.) </summary>
public interface IExcelSheet
{
    /// <summary>Номер первой строки с данными.</summary>
    int StartRow { get; }

    /// <summary>Номер последней строки с данными.</summary>
    int EndRow { get; }

    /// <summary>Номер первого столбца с данными.</summary>
    int StartColumn { get; }

    /// <summary>Номер последнего столбца с данными.</summary>
    int EndColumn { get; }

    /// <summary>Получает текстовое значение ячейки по указанной позиции</summary>
    /// <param name="row">Номер строки (1-based).</param>
    /// <param name="column">Номер столбца (1-based).</param>
    /// <returns>Текстовое значение ячейки, либо пустая строка.</returns>
    string GetCellValue(int row, int column);
}
