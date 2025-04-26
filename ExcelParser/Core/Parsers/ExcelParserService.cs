using System.Collections;
using ExcelParser.Core.Abstractions;
using ExcelParser.Core.Parsers.Flexible;

namespace ExcelParser.Core.Parsers;

/// <summary>реализация сервиса парсинга Excel</summary>
public class ExcelParserService : IExcelParserService
{
    private readonly IExcelSheetFactory _sheetFactory;

    public ExcelParserService(IExcelSheetFactory sheetFactory)
    {
        _sheetFactory = sheetFactory;
    }

    public async Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new()
    {
        return await ExcelParserContext.ParseAsync<TModel>(sheet);
    }

    public async Task<List<TModel>> ParseFromStreamAsync<TModel>(Stream stream, string sheetName) where TModel : new()
    {
        var sheet = await _sheetFactory.CreateFromStreamAsync(stream, sheetName);
        return await ParseAsync<TModel>(sheet);
    }

    public async Task<Dictionary<string, IList>> ParseSheetsWithMappingAsync(Stream stream, Dictionary<string, Type> sheetMappings)
    {
        var sheets = await _sheetFactory.CreateAllSheetsFromStreamAsync(stream);

        var result = new Dictionary<string, IList>();

        foreach (var (sheetName, modelType) in sheetMappings)
        {
            var sheet = sheets.FirstOrDefault(s => s.SheetName.Equals(sheetName, StringComparison.OrdinalIgnoreCase)).Sheet;
            if (sheet == null)
                continue;

            var parsedList = await ParseByTypeAsync(sheet, modelType);
            result.Add(sheetName, parsedList);
        }

        return result;
    }

    public async Task<Dictionary<Type, IList>> ParseFlexibleByColumnsAsync(IExcelSheet sheet, List<ModelColumnsMapping> mappings)
    {
        var result = new Dictionary<Type, IList>();

        // Создание коллекций для каждой модели
        foreach (var mapping in mappings)
        {
            var listType = typeof(List<>).MakeGenericType(mapping.ModelType);
            result[mapping.ModelType] = (IList)Activator.CreateInstance(listType)!;
        }

        // Состояние моделей
        var modelStarted = mappings.ToDictionary(m => m.ModelType, _ => false);
        var modelActive = mappings.ToDictionary(m => m.ModelType, _ => true);
        var modelStartRow = mappings.ToDictionary(m => m.ModelType, m => sheet.StartRow + m.SkipRows);
        var emptyRowCounter = mappings.ToDictionary(m => m.ModelType, _ => 0);

        for (int row = sheet.StartRow + 1; row <= sheet.EndRow; row++)
        {
            foreach (var mapping in mappings)
            {
                if (!modelActive[mapping.ModelType])
                    continue;

                // Пропускаем строки заголовков
                if (row <= modelStartRow[mapping.ModelType])
                    continue;

                bool hasData = mapping.ColumnPropertyMap.Keys
                    .Any(col => !string.IsNullOrWhiteSpace(sheet.GetCellValue(row, col)));

                if (!modelStarted[mapping.ModelType])
                {
                    if (hasData)
                    {
                        // Проверяем StartKeyword, если задан
                        if (!string.IsNullOrWhiteSpace(mapping.StartKeyword))
                        {
                            var firstCol = mapping.ColumnPropertyMap.Keys.First();
                            var checkValue = sheet.GetCellValue(row, firstCol);

                            if (string.IsNullOrWhiteSpace(checkValue) || !checkValue.Contains(mapping.StartKeyword, StringComparison.OrdinalIgnoreCase))
                            {
                                continue; // Ждём корректный старт
                            }
                        }

                        modelStarted[mapping.ModelType] = true;
                        continue; // старт модели зафиксирован
                    }
                    else
                    {
                        continue; // Пока модель не стартовала
                    }
                }

                if (!hasData)
                {
                    emptyRowCounter[mapping.ModelType]++;

                    if (emptyRowCounter[mapping.ModelType] >= mapping.MaxEmptyRows)
                    {
                        modelActive[mapping.ModelType] = false; // Завершаем модель
                    }

                    continue;
                }
                else
                {
                    emptyRowCounter[mapping.ModelType] = 0; // если снова появились данные, обнуляем счётчик
                }

                // Нормальный парсинг строки
                var model = FlexibleRowMapper.MapRowToModel(mapping.ModelType, mapping.ColumnPropertyMap, col => sheet.GetCellValue(row, col));
                if (model != null)
                {
                    result[mapping.ModelType].Add(model);
                }
            }
        }

        return result;
    }

    private async Task<IList> ParseByTypeAsync(IExcelSheet sheet, Type modelType)
    {
        var method = typeof(ExcelParserContext)
            .GetMethod(nameof(ExcelParserContext.ParseAsync))!
            .MakeGenericMethod(modelType);

        var task = (Task)method.Invoke(null, new object[] { sheet })!;
        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result")!;
        return (IList)resultProperty.GetValue(task)!;
    }
}
