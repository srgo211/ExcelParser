namespace ExcelParser.Core.Abstractions;

/// <summary>Интерфейс преобразователя строкового значения в целевой тип</summary>
public interface ITypeConverter
{
    bool CanConvert(Type targetType);
    object? Convert(string value, Type targetType);
}
