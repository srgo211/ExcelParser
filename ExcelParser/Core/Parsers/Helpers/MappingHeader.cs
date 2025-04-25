using ExcelParser.Core.Abstractions;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Helpers;

internal class MappingHeader
{
    public static Dictionary<int, PropertyInfo> Map<TModel>(IExcelSheet sheet)
    {
        var headerMap = new Dictionary<int, PropertyInfo>();

        var startRow = sheet.StartRow;
        var endRow = sheet.EndRow;
        var startCol = sheet.StartColumn;
        var endCol = sheet.EndColumn;

        if (startRow == 0 || endRow == 0 || startCol == 0 || endCol == 0) return headerMap;

        var properties = typeof(TModel).GetProperties();
        var modelType = typeof(TModel);

        // Маппинг заголовков
        for (int col = startCol; col <= endCol; col++)
        {
            var header = NormalizeHeader.Normalize(sheet.GetCellValue(startRow, col));
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var property = ExcelPropertyMatcher.FindMatchingProperty(modelType, properties, header);
            if (property != null)
            {
                headerMap[col] = property;
            }
        }
        return headerMap;
    }
}
