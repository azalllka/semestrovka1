namespace HttpServerLibrary;

public static class SessionStorage
{
    private static readonly Dictionary<string, string> InMemorySessions = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> UserRoles = new Dictionary<string, string>();

    // Сохранить роль пользователя
    public static void SaveRole(string token, string role)
    {
        UserRoles[token] = role;
    }

    // Получить роль пользователя по токену
    public static string GetRole(string token)
    {
        return UserRoles.TryGetValue(token, out var role) ? role : "user";
    }

    public static string? GetUserId(string token)
    {
        return InMemorySessions.TryGetValue(token, out var userId) ? userId : null;
    }
    
    // Пример сохранения токена в память
    public static void SaveSession(string token, string userId)
    {
        InMemorySessions[token] = userId;
    }
    // Пример валидации токена
    public static bool ValidateToken(string token)
    {
        var isValid = InMemorySessions.ContainsKey(token);
        return isValid;
    }

    // Пример удаления сессии
    public static void RemoveSession(string token)
    {
        InMemorySessions.Remove(token);
        UserRoles.Remove(token);
    }
}