namespace HttpServerLibrary.HttpResponse;

/// <summary>
/// Интерфейс, представляющий результат HTTP-ответа.
/// </summary>
public interface IHttpResponseResult
{
    /// <summary>
    /// Выполняет отправку результата HTTP-ответа в рамках указанного HTTP-контекста.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса и ответа.</param>
    void Execute(HttpRequestContext context);
}