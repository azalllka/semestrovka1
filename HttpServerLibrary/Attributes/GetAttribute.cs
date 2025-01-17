/// <summary>
/// Атрибут, обозначающий метод как обработчик HTTP GET-запроса.
/// </summary>
[AttributeUsage(AttributeTargets.Method)] // Указывает, что атрибут применяется только к методам.
public class GetAttribute : Attribute
{
    /// <summary>
    /// URL-роут, обрабатываемый методом.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута <see cref="GetAttribute"/>.
    /// </summary>
    /// <param name="route">URL-роут для обработки.</param>
    public GetAttribute(string route)
    {
        Route = route; // Устанавливаем значение маршрута, который будет связан с методом.
    }
}
