using HttpServerLibrary;

internal class Program
{
    static async Task Main(string[] args)
    {
        //// Доступ к синглтону AppConfig для получения конфигурации приложения
        var config = AppConfig.GetInstance();
        
        // Настройка и запуск HTTP-сервера с префиксами, используя конфигурацию из AppConfig
        var prefixes = new[] { $"http://{AppConfig.Domain}:{AppConfig.Port}/" };
        var server = new HttpServer(prefixes);
        

        // Запуск сервера в асинхронном режиме
        await server.StartAsync();
    }
}
