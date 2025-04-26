using System.Linq.Expressions;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Flexible;


/// Fluent-конфигуратор для маппинга колонок Excel на свойства модели.
/// Позволяет настраивать соответствие колонок, условия старта, пропуск заголовков и допустимое количество пустых строк.
/// </summary>
public class ColumnMapBuilder<TModel>
{
    private readonly Dictionary<int, PropertyInfo> _map = new(); // Маппинг: индекс колонки -> свойство модели
    private string? _startKeyword; // Ключевое слово для старта модели (если задано)
    private int _skipRows = 0; // Количество строк для пропуска до начала чтения данных
    private int _maxEmptyRows = 2; // Максимально допустимое количество пустых строк перед завершением модели

    /// <summary>
    /// Указывает соответствие колонки и свойства модели.
    /// </summary>
    /// <param name="columnIndex">Индекс колонки (начиная с 1)</param>
    /// <param name="propertyExpression">Выражение для выбора свойства модели</param>
    public ColumnMapBuilder<TModel> Map(int columnIndex, Expression<Func<TModel, object>> propertyExpression)
    {
        var propInfo = ExpressionHelper.GetPropertyInfo(propertyExpression);
        _map[columnIndex] = propInfo;
        return this;
    }

    /// <summary>
    /// Указывает текст, наличие которого в ячейке запускает начало парсинга модели.
    /// </summary>
    /// <param name="keyword">Ключевое слово для старта</param>
    public ColumnMapBuilder<TModel> StartWhen(string keyword)
    {
        _startKeyword = keyword;
        return this;
    }

    /// <summary>
    /// Указывает количество строк, которые необходимо пропустить перед началом чтения данных (например, заголовки).
    /// </summary>
    /// <param name="rows">Количество строк для пропуска</param>
    public ColumnMapBuilder<TModel> Skip(int rows)
    {
        _skipRows = rows;
        return this;
    }

    /// <summary>
    /// Указывает допустимое количество подряд идущих пустых строк до завершения обработки модели.
    /// </summary>
    /// <param name="count">Количество допустимых пустых строк</param>
    public ColumnMapBuilder<TModel> AllowEmptyRows(int count)
    {
        _maxEmptyRows = count;
        return this;
    }

    /// <summary>
    /// Собирает все настройки в кортеж для передачи в главный маппинг моделей.
    /// </summary>
    public (Dictionary<int, PropertyInfo> Columns, string? StartKeyword, int SkipRows, int MaxEmptyRows) Build()
    {
        return (_map, _startKeyword, _skipRows, _maxEmptyRows);
    }
}




public class ModelColumnsMappingBuilder
{
    private readonly List<ModelColumnsMapping> _mappings = new();

    /// <summary>
    /// Конфигурация маппинга для конкретной модели.
    /// </summary>
    public ModelColumnsMappingBuilder For<TModel>(Action<ColumnMapBuilder<TModel>> configure)
    {
        var builder = new ColumnMapBuilder<TModel>();
        configure(builder);
        var (columns, startKeyword, skipRows, maxEmptyRows) = builder.Build();
        _mappings.Add(new ModelColumnsMapping(typeof(TModel), columns, startKeyword, skipRows, maxEmptyRows));
        return this;
    }

    /// <summary>
    /// Завершает конфигурацию и строит список маппингов.
    /// </summary>
    public List<ModelColumnsMapping> Build()
    {
        return _mappings;
    }
}



