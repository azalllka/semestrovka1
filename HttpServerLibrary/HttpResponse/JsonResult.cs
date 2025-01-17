using System.Text;
using System.Text.Json;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;

/// <summary>
/// Класс, представляющий результат HTTP-ответа в формате JSON.
/// </summary>
public class JsonResult : IHttpResponseResult
{
    /// <summary>
    /// Данные, которые необходимо сериализовать в JSON для ответа.
    /// </summary>
    private readonly object _data;

    /// <summary>
    /// Создает экземпляр класса <see cref="JsonResult"/> с указанными данными.
    /// </summary>
    /// <param name="data">Объект данных, который будет сериализован в JSON.</param>
    public JsonResult(object data)
    {
        _data = data; // Сохраняем данные, которые будут преобразованы в JSON-формат.
    }

    /// <summary>
    /// Выполняет отправку результата HTTP-ответа в формате JSON.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса и ответа.</param>
    public void Execute(HttpRequestContext context)
    {
        // Сериализуем объект данных в строку JSON.
        var json = JsonSerializer.Serialize(_data);

        // Кодируем строку JSON в массив байтов с использованием UTF-8.
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        // Добавляем заголовок Content-Type для обозначения, что ответ содержит JSON.
        context.Response.Headers.Add("Content-Type", "application/json");

        // Устанавливаем длину содержимого ответа.
        context.Response.ContentLength64 = buffer.Length;

        // Открываем поток для записи данных в ответ.
        using Stream output = context.Response.OutputStream;

        // Записываем закодированные данные в поток.
        output.Write(buffer);

        // Завершаем запись и отправляем данные клиенту.
        output.Flush();
    }
}