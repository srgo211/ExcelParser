using System.Linq.Expressions;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Fluent;

/// <summary> Fluent-конфигуратор для маппинга свойств моделей. </summary>
public class ExcelFluentConfigurator<T>
{
    private readonly ExcelMapping<T> _mapping = new();

    public ExcelFluentConfigurator<T> Map(Expression<Func<T, object>> propertyExpression, string columnName)
    {
        var member = GetPropertyInfo(propertyExpression);
        _mapping.Map(member, columnName);
        return this;
    }

    internal ExcelMapping<T> Build() => _mapping;

    private static PropertyInfo GetPropertyInfo(Expression<Func<T, object>> expression)
    {
        var member = expression.Body as MemberExpression ??
                     ((UnaryExpression)expression.Body).Operand as MemberExpression;

        if (member == null || member.Member.MemberType != MemberTypes.Property)
            throw new ArgumentException("Выражение должно ссылаться на свойство.");

        return (PropertyInfo)member.Member;
    }
}