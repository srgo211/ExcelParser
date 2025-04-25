using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Parsers.Helpers;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

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

        var headerMap = MappingHeader.Map<TModel>(sheet);
        if(!headerMap.Any()) return [];


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
            BoundedCapacity = Environment.ProcessorCount * 10
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
        for (int row = sheet.StartRow + 1; row <= sheet.EndRow; row++)
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
