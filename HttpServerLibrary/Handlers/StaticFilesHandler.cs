using System.Net;
using System.Text;

namespace HttpServerLibrary.Handlers;

/// <summary>
    /// Обработчик для работы с запросами на статические файлы.
    /// </summary>
    internal sealed class StaticFilesHandler : Handler
    {
        /// <summary>
        /// Метод обработки HTTP-запроса на статические файлы.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса, содержащий запрос и ответ.</param>
        public override void HandleRequest(HttpRequestContext context)
        {
            var request = context.Request;

            // Проверка, что метод запроса - GET
            bool isGetRequest = request.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase);

            // Проверка, что запрашивается файл (например, index.html)
            string[] pathParts = request.Url.AbsolutePath.Split(".");
            bool isFileRequest = pathParts.Length >= 2;

            // Если это GET-запрос на файл
            if (isGetRequest && isFileRequest)
            {
                // Формирование полного пути до файла
                string filePath = Path.Combine(AppConfig.StaticDirectoryPath, request.Url.AbsolutePath.TrimStart('/'));
                try
                {
                    // Проверка, существует ли файл
                    if (!File.Exists(filePath))
                    {
                        // Если файл не найден, пытаемся вернуть 404.html
                        filePath = Path.Combine(AppConfig.StaticDirectoryPath, "404.html");

                        // Если 404.html тоже не найден, возвращаем статус 404 и сообщение
                        if (!File.Exists(filePath))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            byte[] notFoundResponse = Encoding.UTF8.GetBytes("404 Not Found");
                            context.Response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                            context.Response.OutputStream.Close();
                            return;
                        }
                        else
                        {
                            // Если 404.html найден, отправляем его
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }

                    // Отправка найденного файла
                    byte[] responseFile = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetContentType(Path.GetExtension(filePath)); // Определение типа контента по расширению
                    context.Response.ContentLength64 = responseFile.Length; // Установка длины содержимого ответа
                    context.Response.OutputStream.Write(responseFile, 0, responseFile.Length); // Запись данных в поток ответа
                }
                catch (FileNotFoundException ex)
                {
                    // Ошибка, если файл не найден
                    Console.WriteLine($"File not found: {ex.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                catch (UnauthorizedAccessException ex)
                {
                    // Ошибка доступа
                    Console.WriteLine($"Access error: {ex.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                catch (Exception ex)
                {
                    // Общая ошибка
                    Console.WriteLine($"Error: {ex.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                finally
                {
                    context.Response.Close();
                }
            }
            // Если это не запрос на файл, передаем его следующему обработчику
            else if (Successor != null)
            {
                Successor.HandleRequest(context);
            }
        }

        /// <summary>
        /// Метод для получения типа контента по расширению файла.
        /// </summary>
        /// <param name="extension">Расширение файла.</param>
        /// <returns>Тип контента, соответствующий расширению файла.</returns>
        private string GetContentType(string? extension)
        {
            if (extension == null)
            {
                // Если расширение null, выбрасываем исключение
                throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
            }

            // Определяем тип контента в зависимости от расширения файла
            return extension.ToLower() switch
            {
                ".html" => "text/html", // Для HTML-файлов
                ".css" => "text/css", // Для CSS-файлов
                ".js" => "application/javascript", // Для JavaScript-файлов
                ".jpg" => "image/jpeg", // Для JPG-изображений
                ".jpeg" => "image/jpeg", // Для JPEG-изображений
                ".png" => "image/png", // Для PNG-изображений
                ".gif" => "image/gif", // Для GIF-изображений
                ".json" => "application/json", // Для JSON
                ".xml" => "application/xml", // Для XML
                _ => "application/octet-stream", // Для всех прочих типов файлов
            };
        }
    }