using System.Data.SqlClient;
using System.Net;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;
using ORMLibrary;
using Rezka.Models;

namespace Rezka.Endpoints;

internal class SignupEndpoint : EndpointBase
{
    private readonly OrmContext<User> _ormContext = new(new SqlConnection(AppConfig.GetInstance().ConnectionString));

    [Get("signup")]
    public IHttpResponseResult GetPage()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "public", "registration.html");
        return Html(File.ReadAllText(filePath));
    }
    
    [Post("signup")]
    public IHttpResponseResult Signup(string login, string password, string email, string role = "user")
    {

        if (role != "admin" && role != "user")
        {
            role = "user"; 
        }

        var existsUser = _ormContext.GetAll("Users").FirstOrDefault(u => u.Login == login);

        if (existsUser != null)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "public", "registration.html");
            var errorMessage = $"User {login} already exists.";
            return Html(File.ReadAllText(filePath) + errorMessage);
        }

        var newUser = new User
        {
            Email = email,
            Login = login,
            Password = password,
            Role = role
        };
        _ormContext.Create(newUser, "Users");

        var token = Guid.NewGuid().ToString();
        var sessionCookie = new Cookie("session-token", token)
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.Now.AddDays(1)
        };
        Context.Response.Cookies.Add(sessionCookie);
        SessionStorage.SaveSession(token, newUser.Id.ToString());
        SessionStorage.SaveRole(token, newUser.Role);

        return Redirect("/");
    }
}