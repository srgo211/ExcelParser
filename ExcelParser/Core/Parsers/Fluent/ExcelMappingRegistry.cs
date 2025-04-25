namespace ExcelParser.Core.Parsers.Fluent;

/// <summary> Реестр всех Fluent-маппингов</summary>
public static class ExcelMappingRegistry
{
    private static readonly Dictionary<Type, IExcelMapping> _mappings = new();

    public static void Register<T>(Action<ExcelFluentConfigurator<T>> configure)
    {
        var configurator = new ExcelFluentConfigurator<T>();
        configure(configurator);
        _mappings[typeof(T)] = configurator.Build();
    }

    public static IExcelMapping? GetMapping(Type modelType)
    {
        _mappings.TryGetValue(modelType, out var mapping);
        return mapping;
    }
}
