using System.Data.SqlClient;
using System.Net;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;
using ORMLibrary;
using Rezka.Models;

namespace Rezka.Endpoints;

internal class SignInEndpoint : EndpointBase
{
    [Get("signin")]
    public IHttpResponseResult GetPage()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "public", "entrance.html");
        return Html(File.ReadAllText(filePath));
    }
    [Post("signin")]
    public IHttpResponseResult SignIn(string login, string password)
    {
        if (IsAuthorized(Context))
        {
            return Redirect("/");
        }

        var context = new OrmContext<User>(new SqlConnection(AppConfig.GetInstance().ConnectionString));
        var user = context.GetAll("Users").FirstOrDefault(u => u.Login == login);

        if (user == null || user.Password != password)   
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "public", "entrance.html");
            var errorMessage = "<p>Неверный логин или пароль.</p>";
            return Html(File.ReadAllText(filePath) + errorMessage);
        }

        // Генерируем токен
        var token = Guid.NewGuid().ToString();
        Cookie nameCookie = new Cookie("session-token", token)
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.Now.AddDays(1)
        };
        Context.Response.Cookies.Add(nameCookie);

        // Сохраняем сессию и роль
        SessionStorage.SaveSession(token, user.Id.ToString());
        SessionStorage.SaveRole(token, user.Role);
        // Redirect based on the role
        if (user.Role == "admin")
        {
            return Redirect("/admin.html"); 
        }
        return Redirect("/");
    }
    
    private bool IsAuthorized(HttpRequestContext context)
    {
        // Проверка наличия куки с session-token
        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Name == "session-token");
        if (cookie != null)
        {
            var isValid = SessionStorage.ValidateToken(cookie.Value);
            return isValid;
        }
        return false;  // Если куки нет или токен невалиден
    }
    public bool IsAdmin(HttpRequestContext context)
    {
        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Name == "session-token");
        if (cookie != null)
        {
            var role = SessionStorage.GetRole(cookie.Value);
            return role == "admin";
        }
        return false;
    }
}