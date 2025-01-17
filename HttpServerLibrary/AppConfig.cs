using System.Text.Json;

namespace HttpServerLibrary;

public sealed class AppConfig
{ 
    public string ConnectionString { get; set; }
    
    public static string Domain { get; set; } = "localhost";
    
    public static uint Port { get; set; } = 8888;
    
    public static string StaticDirectoryPath { get; set; } = "public/";
    
    public const string FileName = "config.json";
    
    /// <summary>
    /// Единственный экземпляр класса <see cref="AppConfig"/>
    /// </summary>
    private static AppConfig _instance;
    
    
    private static AppConfig LoadConfig()
    {
        if (File.Exists(FileName))
        {
            try
            {
                var configFile = File.ReadAllText(FileName);
                return JsonSerializer.Deserialize<AppConfig>(configFile) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении конфигурации: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Файл {FileName} не найден. Используются настройки по умолчанию.");
        }
        return new AppConfig();
    }
    
    /// <summary>
    /// Инициализирует instance с данными из файла
    /// если файл не существует выводить сообщение в консоль
    /// </summary>
    private void Initialize()
    {
        if (File.Exists(FileName))
        {
            try
            {
                var configFile = File.ReadAllText(FileName);
                var config = JsonSerializer.Deserialize<AppConfig>(configFile);

                if (config != null)
                {
                    ConnectionString = config.ConnectionString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации конфигурации: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Файл настроек {FileName} не найден.");
        }
    }
    
    /// <summary>
    /// Проверяет наличие экземпляра класса <see cref="AppConfig"/>
    /// Инициализирует его, если экземпляра раннее не существовало
    /// </summary>
    /// <returns>Instance of <see cref="AppConfig"/></returns>
    public static AppConfig GetInstance()
    {
        if (_instance is null)
        {
            _instance = new AppConfig();
            _instance.Initialize();
        }

        return _instance;
    }
}