using HttpServerLibrary.HttpResponse;

namespace HttpServerLibrary;

/// <summary>
/// Базовый класс для реализации конечных точек (endpoints) сервера.
/// </summary>
public abstract class EndpointBase
{
    /// <summary>
    /// Контекст текущего HTTP-запроса.
    /// </summary>
    protected HttpRequestContext Context { get; private set; }
    protected IHttpResponseResult Redirect(string url) => new RedirectResult(url);

    /// <summary>
    /// Устанавливает контекст текущего запроса.
    /// </summary>
    /// <param name="context">Контекст запроса, содержащий информацию о запросе и ответе.</param>
    internal void SetContext(HttpRequestContext context)
    {
        // Присваиваем переданный контекст свойству Context.
        Context = context;
    }

    /// <summary>
    /// Создает HTML-ответ на основе переданного HTML-контента.
    /// </summary>
    /// <param name="html">Строка с содержимым HTML-документа.</param>
    /// <returns>Результат HTTP-ответа с типом контента "text/html".</returns>
    protected IHttpResponseResult Html(string html) => new HtmlResult(html);

    // Здесь создается экземпляр HtmlResult для передачи HTML-контента в качестве ответа клиенту.

    /// <summary>
    /// Создает JSON-ответ на основе переданных данных.
    /// </summary>
    /// <param name="data">Объект, который будет сериализован в JSON.</param>
    /// <returns>Результат HTTP-ответа с типом контента "application/json".</returns>
    protected IHttpResponseResult Json(object data) => new JsonResult(data);

    // Здесь создается экземпляр JsonResult, чтобы вернуть объект в виде JSON-строки.
}