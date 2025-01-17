using System.Net;

namespace HttpServerLibrary;

/// <summary>
/// Класс, представляющий контекст текущего HTTP-запроса, содержащий запрос и ответ.
/// </summary>
public sealed class HttpRequestContext
{
    /// <summary>
    /// HTTP-запрос, отправленный клиентом.
    /// </summary>
    public HttpListenerRequest Request { get; }

    /// <summary>
    /// HTTP-ответ, который будет отправлен клиенту.
    /// </summary>
    public HttpListenerResponse Response { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="HttpRequestContext"/> с указанным запросом и ответом.
    /// </summary>
    /// <param name="request">HTTP-запрос от клиента.</param>
    /// <param name="response">HTTP-ответ для клиента.</param>
    public HttpRequestContext(HttpListenerRequest request, HttpListenerResponse response)
    {
        // Присваиваем переданный запрос свойству Request.
        Request = request;

        // Присваиваем переданный ответ свойству Response.
        Response = response;
    }
}