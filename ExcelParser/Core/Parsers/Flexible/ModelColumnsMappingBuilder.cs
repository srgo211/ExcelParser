using System.Linq.Expressions;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Flexible;

/// <summary>
/// Fluent-настройка маппинга колонок.
/// </summary>
public class ColumnMapBuilder<TModel>
{
    private readonly Dictionary<int, PropertyInfo> _map = new();
    private string? _startKeyword;
    private int _skipRows = 0;

    public ColumnMapBuilder<TModel> Map(int columnIndex, Expression<Func<TModel, object>> propertyExpression)
    {
        var propInfo = ExpressionHelper.GetPropertyInfo(propertyExpression);
        _map[columnIndex] = propInfo;
        return this;
    }

    public ColumnMapBuilder<TModel> StartWhen(string keyword)
    {
        _startKeyword = keyword;
        return this;
    }

    public ColumnMapBuilder<TModel> Skip(int rows)
    {
        _skipRows = rows;
        return this;
    }

    public (Dictionary<int, PropertyInfo> Columns, string? StartKeyword, int SkipRows) Build()
    {
        return (_map, _startKeyword, _skipRows);
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

        var (columns, startKeyword, skipRows) = builder.Build();

        _mappings.Add(new ModelColumnsMapping(
            typeof(TModel),
            columns,
            startKeyword,
            skipRows
        ));

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



