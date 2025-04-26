using System.Linq.Expressions;
using System.Reflection;

namespace ExcelParser.Core.Parsers.Flexible;

/// <summary>
/// Помощник для работы с лямбда-выражениями.
/// Извлекает PropertyInfo.
/// </summary>
public static class ExpressionHelper
{
    public static PropertyInfo GetPropertyInfo<TModel>(Expression<Func<TModel, object>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            // Прямое свойство (например, x => x.Name)
            return ValidateMember(member);
        }

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
        {
            // При необходимости приведение типов (например, x => (object)x.Age)
            return ValidateMember(unaryMember);
        }

        throw new ArgumentException("Выражение должно быть ссылкой на свойство.");
    }

    private static PropertyInfo ValidateMember(MemberExpression member)
    {
        if (member.Member is PropertyInfo propertyInfo)
            return propertyInfo;

        throw new ArgumentException("Выражение должно быть ссылкой на свойство, а не на поле или метод.");
    }
}