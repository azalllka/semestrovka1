namespace HttpServerLibrary.Handlers;

    /// <summary>
    /// Абстрактный класс обработчика, который служит базой для реализации цепочки обработчиков.
    /// </summary>
    abstract class Handler
    {
        /// <summary>
        /// Следующий обработчик в цепочке ответственности. Используется для передачи запроса следующему обработчику,
        /// если текущий обработчик не может его обработать.
        /// </summary>
        public Handler Successor { get; set; }

        /// <summary>
        /// Абстрактный метод, который должен быть реализован в дочерних классах для обработки HTTP-запросов.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса, содержащий запрос и ответ.</param>
        public abstract void HandleRequest(HttpRequestContext context);
    }