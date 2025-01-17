/// <summary>
/// Атрибут, обозначающий метод как обработчик HTTP POST-запроса.
/// </summary>
public class PostAttribute : Attribute
{
    /// <summary>
    /// URL-роут, обрабатываемый методом.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута <see cref="PostAttribute"/>.
    /// </summary>
    /// <param name="route">URL-роут для обработки.</param>
    public PostAttribute(string route)
    {
        Route = route; // Устанавливаем значение маршрута, который будет связан с методом.
    }
}
