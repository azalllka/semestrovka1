using System.Data;
using System.Linq.Expressions;
using MyORMLibrary;

namespace ORMLibrary;

public class OrmContext<T> where T : class, new()
{
    private readonly IDbConnection _dbConnection;

    public OrmContext(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }
    public List<T> Search(string tableName, string searchTerm)
    {
        List<T> results = new List<T>();

        // Construct the SQL query for searching by Title (you can modify the column as needed)
        string condition = "Title LIKE @searchTerm";
        var parameters = new Dictionary<string, object>
        {
            { "@searchTerm", $"%{searchTerm}%" }
        };

        string sql = $"SELECT * FROM {EscapeName(tableName)} WHERE {condition}";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sql;

                // Add parameters to the command
                foreach (var param in parameters)
                {
                    var sqlParam = command.CreateParameter();
                    sqlParam.ParameterName = param.Key;
                    sqlParam.Value = param.Value ?? DBNull.Value;
                    command.Parameters.Add(sqlParam);
                }

                _dbConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(Map(reader)); // Map the reader result to your model
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return results;
    }
    public T GetById(int id, string tableName)
    {
        // Строим SQL запрос для получения записи по ID
        string query = $"SELECT * FROM {EscapeName(tableName)} WHERE Id = @Id";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;

                // Создаем параметр для запроса
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                _dbConnection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader); // Возвращаем объект, преобразованный из строки результата
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return null; // Возвращаем null, если запись не найдена
    }
    private static string EscapeName(string name)
    {
        return $"[{name}]";
    }
    public T ReadById(int Id)
    {
        string query = $"SELECT * FROM {EscapeName(typeof(T).Name)}s WHERE Id = @Id";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = query;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = Id;
                command.Parameters.Add(parameter);

                _dbConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return null;
    }

   public T Create(T entity, string tableName)
{
    var properties = typeof(T).GetProperties();
    var escapedColumnNames = string.Join(", ", properties.Where(p => p.Name != "Id").Select(p => EscapeName(p.Name)));
    var parameterNames = string.Join(", ", properties.Where(p => p.Name != "Id").Select(p => "@" + p.Name));

    string sql = $"INSERT INTO {EscapeName(tableName)} ({escapedColumnNames}) VALUES ({parameterNames}); SELECT SCOPE_IDENTITY();";

    try
    {
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = sql;

            // Добавляем параметры
            foreach (var property in properties.Where(p => p.Name != "Id")) // Пропускаем Id
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{property.Name}";
                parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            _dbConnection.Open();

            // Выполняем запрос и получаем ID добавленной записи
            var result = command.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                var idProperty = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                idProperty?.SetValue(entity, newId); // Устанавливаем ID нового объекта
            }
        }
    }
    catch (Exception ex)
    {
        // Логируем ошибку
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        _dbConnection.Close();
    }
    
    return entity;
}

    public List<T> GetAll(string tableName, string orderByColumn = null, int? take = null, string whereClause = null, Dictionary<string, object> parameters = null)
    {
        var sql = $"SELECT * FROM {EscapeName(tableName)}";

        if (!string.IsNullOrEmpty(whereClause))
        {
            sql += $" WHERE {whereClause}";
        }

        if (!string.IsNullOrEmpty(orderByColumn))
        {
            sql += $" ORDER BY {orderByColumn} DESC";
        }

        if (take.HasValue)
        {
            sql = sql.Replace("SELECT", $"SELECT TOP {take.Value}");
        }

        var results = new List<T>();

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sql;

                // Добавление параметров для предотвращения SQL-инъекций
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var sqlParam = command.CreateParameter();
                        sqlParam.ParameterName = param.Key;
                        sqlParam.Value = param.Value;
                        command.Parameters.Add(sqlParam);
                    }
                }

                _dbConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(Map(reader));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return results;
    }
    public void Update(int id, T entity, string tableName)
    {
        var properties = typeof(T).GetProperties();
         var setClause = string.Join(", ", properties.Select(p => $"{EscapeName(p.Name)} = @{p.Name}"));
        string sql = $"UPDATE {EscapeName(tableName)} SET {setClause} WHERE Id = @Id";
        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sql;
                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = property.GetValue(entity) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }

                var idParameter = command.CreateParameter();
                idParameter.ParameterName = "@Id";
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                _dbConnection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    public void Delete(int id, string tableName)
    {
        string sql = $"DELETE FROM {EscapeName(tableName)} WHERE Id = @Id";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sql;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                _dbConnection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }
    }

    private T Map(IDataReader reader)
    {
        var obj = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
             var value = reader[property.Name];
             if (value != DBNull.Value)
             {
                 try
                 {
                    
                     if (property.Name == "KinopoiskRating")
                     {
                         if (value is double doubleValue)
                         {
                             property.SetValue(obj, (float)doubleValue);
                         }
                         else
                         {
                             // Для других типов, если необходимо
                             Console.WriteLine($"KinopoiskRating cannot be cast to float, current type: {value?.GetType().Name}");
                         }
                     }
                     else
                     {
                         // Преобразование для других свойств
                         property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                     }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"Error mapping property {property.Name}: {ex.Message}");
                 }
             }
             else
             {
                 Console.WriteLine($"Property {property.Name} is null in the database.");
             }
        }

        return obj;
    }

    public List<T> Where(string condition, string tableName, Dictionary<string, object> parameters = null)
    {
        List<T> results = new List<T>();
        string sql = $"SELECT * FROM {EscapeName(tableName)} WHERE {condition}";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sql;

                // Add parameters to the command if any are provided
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var sqlParam = command.CreateParameter();
                        sqlParam.ParameterName = param.Key;
                        sqlParam.Value = param.Value ?? DBNull.Value;
                        command.Parameters.Add(sqlParam);
                    }
                }

                _dbConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(Map(reader));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return results;
    }
    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var parameters = new Dictionary<string, object>();
        var sqlQuery = ExpressionParser<T>.BuildSqlQuery(predicate, singleResult: true, ref parameters);
         Console.WriteLine($"Generated SQL Query: {sqlQuery}");
         Console.WriteLine($"Generated Parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
        return ExecuteQuerySingle(sqlQuery, parameters);
    }

    public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
    {
        var parameters = new Dictionary<string, object>();
        var sqlQuery = ExpressionParser<T>.BuildSqlQuery(predicate, singleResult: false, ref parameters);
        return ExecuteQueryMultiple(sqlQuery, parameters).ToList();
    }
    
    public T FirstOrDefaultByLogin(string login)
    {
        string sqlQuery = $"SELECT * FROM Users WHERE Login = @Login";

        try
        {
            using (var command = _dbConnection.CreateCommand())
            {
                command.CommandText = sqlQuery;
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Login";
                parameter.Value = login;
                command.Parameters.Add(parameter);

                _dbConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            _dbConnection.Close();
        }

        return null;
    }
    private T ExecuteQuerySingle(string query, Dictionary<string, object> parameters)
    {
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;

            // Add parameters to the command
            foreach (var param in parameters)
            {
                var sqlParam = command.CreateParameter();
                sqlParam.ParameterName = param.Key;
                sqlParam.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(sqlParam);
            }

            _dbConnection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Map(reader);
                }
            }
            _dbConnection.Close();
        }

        return null;
    }

    private IEnumerable<T> ExecuteQueryMultiple(string query, Dictionary<string, object> parameters)
    {
        var results = new List<T>();
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = query;

            // Add parameters to the command
            foreach (var param in parameters)
            {
                var sqlParam = command.CreateParameter();
                sqlParam.ParameterName = param.Key;
                sqlParam.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(sqlParam);
            }

            _dbConnection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(Map(reader));
                }
            }
            _dbConnection.Close();
        }

        return results;
    }
}
