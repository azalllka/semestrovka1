using System.Net;
using HttpServerLibrary.Handlers;

namespace HttpServerLibrary;

/// <summary>
/// Класс, представляющий HTTP-сервер, обрабатывающий запросы и маршруты.
/// </summary>
public sealed class HttpServer
{
    private readonly string _staticDirectoryPath;
    private readonly HttpListener _listener;

    private readonly Handler _staticFilesHandler;
    private readonly Handler _endpointsHandler;

    /// <summary>
    /// Конструктор для инициализации HTTP-сервера с заданными префиксами.
    /// </summary>
    /// <param name="prefixes">Массив префиксов (URL), на которых сервер будет слушать входящие запросы.</param>
    public HttpServer(string[] prefixes)
    {
        _listener = new HttpListener(); // Создание нового слушателя для HTTP-запросов

        foreach (string prefix in prefixes)
        {
            Console.WriteLine($"Prefixe: {prefix}"); // Вывод каждого префикса в консоль
            _listener.Prefixes.Add(prefix); // Добавление каждого префикса в список слушателя
        }

        _staticFilesHandler = new StaticFilesHandler(); // Инициализация обработчика статических файлов
        _endpointsHandler = new EndpointsHandler(); // Инициализация обработчика конечных точек
    }

    /// <summary>
    /// Запускает HTTP-сервер для прослушивания входящих запросов.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию запуска сервера.</returns>
    public async Task StartAsync()
    {
        _listener.Start(); // Запуск HTTP-сервера на всех добавленных префиксах
        Console.WriteLine("Сервер запущен и ожидает запросов..."); // Сообщение о запуске сервера

        while (_listener.IsListening) // Пока сервер продолжает прослушивать запросы
        {
            var context = await _listener.GetContextAsync(); // Ожидание и получение контекста запроса
            var requestContext = new HttpRequestContext(context.Request, context.Response); // Создание контекста запроса для обработки

            await ProcessRequest(requestContext); // Обработка запроса асинхронно
        }
    }

    /// <summary>
    /// Обрабатывает входящий запрос.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса и ответа.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки запроса.</returns>
    private async Task ProcessRequest(HttpRequestContext context)
    {
        _staticFilesHandler.Successor = _endpointsHandler; // Устанавливаем следующий обработчик в цепочке
        _staticFilesHandler.HandleRequest(context); // Обработка запроса с помощью обработчика статических файлов
    }

    /// <summary>
    /// Останавливает HTTP-сервер.
    /// </summary>
    public void Stop()
    {
        _listener.Stop(); // Остановка HTTP-сервера
        Console.WriteLine("Сервер остановлен."); // Сообщение об остановке сервера
    }
}