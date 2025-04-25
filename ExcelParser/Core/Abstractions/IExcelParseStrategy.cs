namespace ExcelParser.Core.Abstractions;

public interface IExcelParseStrategy
{
    Task<List<TModel>> ParseAsync<TModel>(IExcelSheet sheet) where TModel : new();
}
