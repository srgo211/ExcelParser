

using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Attributes;
using ExcelParser.Core.Parsers.Helpers;

namespace ExcelParser.Core.Parsers.Strategies;

/// <summary>
/// Стратегия асинхронного парсинга через Dataflow pipeline.
/// Используется для очень больших файлов.
/// </summary>
public sealed class DataflowParseStrategy : IExcelParseStrategy
{
    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        var result = new ConcurrentDictionary<int, TModel>();

        var startRow = sheet.StartRow;
        var endRow = sheet.EndRow;
        var startCol = sheet.StartColumn;
        var endCol = sheet.EndColumn;

        if (startRow == 0 || endRow == 0 || startCol == 0 || endCol == 0)
            return [];

        var headerMap = new Dictionary<int, PropertyInfo>();
        var properties = typeof(TModel).GetProperties();

        // Маппинг заголовков
        for (int col = startCol; col <= endCol; col++)
        {
            var header = sheet.GetCellValue(startRow, col).Trim();
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var property = properties.FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ExcelColumnAttribute>();
                return attr != null ? attr.ColumnName.Equals(header, StringComparison.OrdinalIgnoreCase) : p.Name.Equals(header, StringComparison.OrdinalIgnoreCase);
            });

            if (property != null)
            {
                headerMap[col] = property;
            }
        }

        // Блок обработки строк
        var transformBlock = new TransformBlock<int, (int Row, TModel Item)?>(row =>
        {
            var model = new TModel();

            foreach (var (col, prop) in headerMap)
            {
                var cellValue = sheet.GetCellValue(row, col);

                if (string.IsNullOrWhiteSpace(cellValue))
                    continue;

                try
                {
                    object? converted = ConvertHelper.ConvertTo(cellValue, prop.PropertyType);
                    prop.SetValue(model, converted);
                }
                catch
                {
                    // Ошибки конвертации можно логировать
                }
            }

            return (row, model);
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            BoundedCapacity = Environment.ProcessorCount * 10 // ограничение очереди
        });

        // Блок сбора результатов
        var actionBlock = new ActionBlock<(int Row, TModel Item)?>(pair =>
        {
            if (pair != null)
                result.TryAdd(pair.Value.Row, pair.Value.Item);
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });

        transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // Подача строк в pipeline
        for (int row = startRow + 1; row <= endRow; row++)
        {
            await transformBlock.SendAsync(row);
        }

        transformBlock.Complete();
        await actionBlock.Completion;

        return result
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();
    }
}
