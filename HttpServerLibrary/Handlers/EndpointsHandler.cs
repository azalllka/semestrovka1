using System.Net;
using System.Reflection;
using HttpServerLibrary.HttpResponse;

namespace HttpServerLibrary.Handlers;

    /// <summary>
    /// Класс, обрабатывающий маршруты запросов и вызывающий соответствующие методы контроллеров.
    /// </summary>
    internal sealed class EndpointsHandler : Handler
    {
        /// <summary>
        /// Словарь для хранения маршрутов с методами и типами конечных точек.
        /// </summary>
        private readonly Dictionary<string, List<(HttpMethod method, MethodInfo methodInfo, Type endpointType)>> 
            _routes = new();

        /// <summary>
        /// Конструктор обработчика, регистрирует конечные точки из текущей сборки.
        /// </summary>
        public EndpointsHandler()
        {
            // Регистрируем конечные точки из текущей сборки
            RegisterEndpointsFromAssemblies(new[] { Assembly.GetEntryAssembly() });
        }

        /// <summary>
        /// Обрабатывает HTTP-запрос, ищет соответствующий маршрут и вызывает нужный метод.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        public override void HandleRequest(HttpRequestContext context)
        {
            var request = context.Request; // Получаем запрос из контекста
            var url = request.Url.LocalPath.Trim('/'); // Извлекаем путь из URL
            var requestMethod = request.HttpMethod; // Извлекаем метод запроса (GET, POST и т.д.)

            // Если для данного маршрута есть зарегистрированные обработчики
            if (_routes.ContainsKey(url))
            {
                // Ищем первый подходящий обработчик по методу (GET, POST и т.д.)
                var route = _routes[url].FirstOrDefault(r =>
                    r.method.ToString().Equals(requestMethod, StringComparison.OrdinalIgnoreCase));

                // Если найден метод для обработки
                if (route.methodInfo != null)
                {
                    // Создаем экземпляр обработчика конечной точки
                    var endpointInstance = Activator.CreateInstance(route.endpointType) as EndpointBase;

                    if (endpointInstance != null)
                    {
                        endpointInstance.SetContext(context); // Устанавливаем контекст для конечной точки

                        // Извлекаем параметры метода
                        var parameters = GetMethodParameters(route.methodInfo, context);

                        // Вызываем метод обработчика с параметрами и получаем результат
                        var result = route.methodInfo.Invoke(endpointInstance, parameters) as IHttpResponseResult;

                        // Если результат не null, выполняем его (отправляем ответ)
                        result?.Execute(context);
                    }
                }
            }
            else if (Successor != null)
            {
                // Если маршрута не найдено, передаем запрос следующему обработчику в цепочке
                Successor.HandleRequest(context);
            }
            else
            {
                // Если обработчик не найден, возвращаем ошибку 404
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                using var writer = new StreamWriter(context.Response.OutputStream);
                writer.Write("404 Not Found");
            }
        }

        /// <summary>
        /// Извлекает параметры из запроса в зависимости от HTTP-метода (GET или POST).
        /// </summary>
        /// <param name="request">HTTP-запрос.</param>
        /// <param name="method">HTTP-метод (GET или POST).</param>
        /// <returns>Параметры запроса, как объект.</returns>
        private object ExtractParameters(HttpListenerRequest request, HttpMethod method)
        {
            // Если метод GET, извлекаем параметры из строки запроса
            if (method == HttpMethod.Get)
            {
                return request.QueryString.AllKeys.ToDictionary(key => key, key => request.QueryString[key]);
            }
            // Если метод POST, извлекаем параметры из тела запроса
            else if (method == HttpMethod.Post)
            {
                using var reader = new StreamReader(request.InputStream);
                var body = reader.ReadToEnd();
                var formData = body.Split('&')
                    .Select(part => part.Split('='))
                    .Where(parts => parts.Length == 2)
                    .ToDictionary(parts => WebUtility.UrlDecode(parts[0]), parts => WebUtility.UrlDecode(parts[1]));
                return formData;
            }

            return null;
        }

        /// <summary>
        /// Регистрирует маршруты из сборок, создавая связи между URL, HTTP-методами и методами обработчиков.
        /// </summary>
        /// <param name="assemblies">Массив сборок для анализа.</param>
        private void RegisterEndpointsFromAssemblies(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                // Получаем типы классов, которые наследуют EndpointBase и не являются абстрактными
                var endpointTypes = assembly.GetTypes()
                    .Where(t => typeof(EndpointBase).IsAssignableFrom(t) && !t.IsAbstract);

                // Для каждого типа в сборке, проверяем методы с атрибутами Get или Post
                foreach (var endpointType in endpointTypes)
                {
                    foreach (var methodInfo in endpointType.GetMethods())
                    {
                        foreach (var attribute in methodInfo.GetCustomAttributes<Attribute>())
                        {
                            // Если атрибут GET, регистрируем маршрут для метода GET
                            if (attribute is GetAttribute getAttr)
                            {
                                RegisterRoute(getAttr.Route, HttpMethod.Get, methodInfo, endpointType);
                            }
                            // Если атрибут POST, регистрируем маршрут для метода POST
                            else if (attribute is PostAttribute postAttr)
                            {
                                RegisterRoute(postAttr.Route, HttpMethod.Post, methodInfo, endpointType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Регистрирует маршрут для указанного пути, метода и обработчика.
        /// </summary>
        /// <param name="route">Маршрут.</param>
        /// <param name="method">HTTP-метод (GET, POST и т.д.).</param>
        /// <param name="methodInfo">Метод, который будет вызван.</param>
        /// <param name="endpointType">Тип обработчика конечной точки.</param>
        private void RegisterRoute(string route, HttpMethod method, MethodInfo methodInfo, Type endpointType)
        {
            // Проверяем, если маршрут уже зарегистрирован
            if (_routes.TryGetValue(route, out var existingRoutes))
            {
                // Если уже есть такой маршрут с этим методом, выбрасываем исключение
                if (existingRoutes.Any(r => r.method == method))
                {
                    string errorMessage =
                        $"Ошибка: Попытка зарегистрировать маршрут \"{route}\" с методом \"{method}\" повторно.";
                    Console.WriteLine(errorMessage); // Логируем ошибку
                    throw new InvalidOperationException(errorMessage); // Прерываем выполнение
                }
            }
            else
            {
                // Если маршрут не зарегистрирован, создаем новый список маршрутов
                _routes[route] = new();
            }

            // Добавляем новый маршрут
            _routes[route].Add((method, methodInfo, endpointType));
        }

        /// <summary>
        /// Извлекает параметры для метода из запроса (GET или POST).
        /// </summary>
        /// <param name="method">Метод, для которого извлекаются параметры.</param>
        /// <param name="context">Контекст запроса.</param>
        /// <returns>Массив значений параметров.</returns>
        private object[] GetMethodParameters(MethodInfo method, HttpRequestContext context)
        {
            var parameters = method.GetParameters(); // Получаем параметры метода
            var values = new object[parameters.Length]; // Массив значений для параметров

            // Если запрос GET, извлекаем параметры из строки запроса
            if (context.Request.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            {
                var queryParameters = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramName = parameters[i].Name;
                    var paramType = parameters[i].ParameterType;
                    var value = queryParameters[paramName];
                    values[i] = ConvertValue(value, paramType); // Преобразуем строковое значение в тип параметра
                }
            }
            // Если запрос POST, извлекаем параметры из тела запроса
            else if (context.Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                using var reader = new StreamReader(context.Request.InputStream);
                var body = reader.ReadToEnd();

                // Если контент-тип "application/x-www-form-urlencoded", извлекаем параметры формы
                if (context.Request.ContentType == "application/x-www-form-urlencoded")
                {
                    var formParameters = System.Web.HttpUtility.ParseQueryString(body);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var paramName = parameters[i].Name;
                        var paramType = parameters[i].ParameterType;
                        var value = formParameters[paramName];
                        values[i] = ConvertValue(value, paramType); // Преобразуем строковое значение в тип параметра
                    }
                }
                // Если контент-тип "application/json", десериализуем тело как JSON
                else if (context.Request.ContentType == "application/json")
                {   
                    Console.WriteLine($"Тело запроса: {body}");

                    var jsonObject = System.Text.Json.JsonSerializer.Deserialize(body, method.GetParameters()[0].ParameterType);
                    return new[] { jsonObject }; // Возвращаем десериализованный объект
                }
            }

            return values;
        }

        /// <summary>
        /// Преобразует строковое значение в указанный тип.
        /// </summary>
        /// <param name="value">Строковое значение.</param>
        /// <param name="targetType">Тип, в который нужно преобразовать.</param>
        /// <returns>Преобразованное значение.</returns>
        private object ConvertValue(string value, Type targetType)
        {
            // Если значение null и тип значения является значимым типом, создаем его значение по умолчанию
            if (value == null)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            // Преобразуем строковое значение в нужный тип
            return Convert.ChangeType(value, targetType);
        }
    }