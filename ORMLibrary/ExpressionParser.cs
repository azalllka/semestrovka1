using System.Linq.Expressions;
using System.Text;

namespace MyORMLibrary;

public static class ExpressionParser<T>
{
    internal static string ParseExpression(Expression expression, ref Dictionary<string, object> parameters)
    {
        if (expression is BinaryExpression binary)
        {
            var left = ParseExpression(binary.Left, ref parameters);
            var right = ParseExpression(binary.Right, ref parameters);
            var op = GetSqlOperator(binary.NodeType);

            return $"({left} {op} {right})";
        }
        else if (expression is MemberExpression member)
        {
            // Проверяем, является ли это параметром из левой части выражения (например, f.UserId)
            if (member.Expression is ParameterExpression)
            {
                return member.Member.Name;
            }

            // Если это свойство, извлекаем его значение
            var constantValue = GetValueFromExpression(member);
            string paramName = $"@param{parameters.Count}";
            parameters[paramName] = constantValue ?? DBNull.Value;
            return paramName;
        }
        else if (expression is ConstantExpression constant)
        {
            string paramName = $"@param{parameters.Count}";
            parameters[paramName] = constant.Value ?? DBNull.Value;
            return paramName;
        }

        throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
    }
    private static object GetValueFromExpression(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }
 
    private static string GetSqlOperator(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.AndAlso => "AND",
            ExpressionType.Or => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };
    }
 
    private static string FormatConstant(object value)
    {
        return value is string ? $"'{value}'" : value.ToString();
    }

    internal static string BuildSqlQuery(Expression<Func<T, bool>> predicate, bool singleResult, ref Dictionary<string, object> parameters)
    {
        var query = new StringBuilder($"SELECT * FROM {typeof(T).Name}s WHERE ");
        query.Append(ParseExpression(predicate.Body, ref parameters));
        return query.ToString();
    }
    private static string MapColumnName(string propertyName)
    {
        return propertyName switch
        {
            "UserId" => "user_id",
            "MovieId" => "movie_id",
            _ => propertyName
        };
    }
}