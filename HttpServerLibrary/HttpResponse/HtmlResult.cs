using System.Text;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;

/// <summary>
/// Класс, представляющий HTML-ответ для HTTP-запроса.
/// </summary>
public class HtmlResult : IHttpResponseResult
{
    /// <summary>
    /// Поле для хранения HTML-контента, который будет отправлен клиенту.
    /// </summary>
    private readonly string _html;

    /// <summary>
    /// Инициализирует экземпляр <see cref="HtmlResult"/> с заданным HTML-контентом.
    /// </summary>
    /// <param name="html">HTML-контент, который будет отправлен в ответ на запрос.</param>
    public HtmlResult(string html)
    {
        // Сохраняем переданный HTML-контент в поле _html.
        _html = html;
    }

    /// <summary>
    /// Возвращает HTML-контент, предназначенный для отправки клиенту.
    /// </summary>
    public string Content => _html;

    /// <summary>
    /// Выполняет отправку HTML-ответа в рамках указанного HTTP-контекста.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса и ответа.</param>
    public void Execute(HttpRequestContext context)
    {
        // Преобразуем HTML-контент в массив байтов, используя кодировку UTF-8.
        byte[] buffer = Encoding.UTF8.GetBytes(_html);

        // Указываем длину содержимого ответа в байтах.
        context.Response.ContentLength64 = buffer.Length;

        // Получаем поток, связанный с ответом.
        using Stream output = context.Response.OutputStream;

        // Пишем содержимое в поток, отправляя данные клиенту.
        output.Write(buffer);

        // Принудительно очищаем и завершаем поток, чтобы данные гарантированно дошли до клиента.
        output.Flush();
    }
}