using System.Data.SqlClient;
using System.Text.Json;
using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;
using Microsoft.AspNetCore.Mvc;
using ORMLibrary;
using Rezka.Models;

namespace Rezka.Endpoints;

internal class MoviesEndpoint : EndpointBase
{
    private readonly OrmContext<Movie> _context;
    private readonly OrmContext<User> _userContext;
    private readonly OrmContext<Favorite> _favContext;
    public MoviesEndpoint()
    {
      
        var connection = new SqlConnection(AppConfig.GetInstance().ConnectionString);
        _context = new OrmContext<Movie>(connection);
        _userContext = new OrmContext<User>(connection);
        _favContext = new OrmContext<Favorite>(connection);
        
    }


    [Get("")]
    public IHttpResponseResult GetMovies(string search = "")
    {
        List<Movie> movies;

        if (string.IsNullOrEmpty(search))
        {
            movies = _context.GetAll("Movies", orderByColumn: "KinopoiskRating", take: 12);
        }
        else
        {
            // Use the new Search method from OrmContext
            movies = _context.Search("Movies", search).ToList();
        }

        var moviesHtml = string.Join("", movies.Select(movie => $@"
        <div class='movie-card'>
            <a href='/movie?id={movie.Id}'>
                <div class='movie-thumbnail'>
                    <img src='{movie.PosterUrl}' alt='{movie.Title}'>
                    <div class='badge'>Фильм ({movie.KinopoiskRating:F2})</div>
                    <div class='play-icon'>▶</div>
                </div>
                <div class='movie-info'>
                    <h3>{movie.Title}</h3>
                    <p>{movie.ReleaseDate}, {movie.Country}, {movie.Genre}</p>
                </div>
            </a>
        </div>"));

        var filePath = Path.Combine(AppContext.BaseDirectory, "public", "index.html");
        var html = File.ReadAllText(filePath);

        html = html.Replace("<!-- Карточка фильма -->", moviesHtml);

        return Html(html);
    }

    [Get("movie")]
    public IHttpResponseResult GetMovieDetails(int id)
    {
        
        var movie = _context.GetById(id, "Movies");
        if (movie == null)
        {
            return Html("<h1>Фильм не найден</h1>");
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "public", "movie.html");
        if (!File.Exists(filePath))
        {
            return Html("<h1>Страница фильма отсутствует</h1>");
        }

        
        var html = File.ReadAllText(filePath);

        // Проверяем авторизацию пользователя
        var user = GetAuthorizedUser(Context);
        // Генерация HTML для фильма
        var movieHtml = $@"
            <h1>{movie.Title}</h1>
            <div class='movie-details'>
                <div class='movie-poster'>
                    <img src='{movie.PosterUrl}' alt='{movie.Title}'>
                </div>
                <div class='movie-info'>
                <p><span class='rating'>Рейтинг:</span> Кинопоиск: <strong>{movie.KinopoiskRating:F2}</strong> ({movie.KinopoiskVotes} голосов)</p>
                <p><span>Входит в списки:</span> {movie.Lists}</p>
                <p><strong>Дата выхода:</strong> {movie.ReleaseDate}</p>
                <p><strong>Страна:</strong> {movie.Country}</p>
                <p><strong>Режиссер:</strong> {movie.Director}</p>
                <p><strong>Жанр:</strong> {movie.Genre}</p>
                <p><strong>В качестве:</strong> {movie.Quality}</p>
                <p><strong>Возраст:</strong> {movie.AgeRating}</p>
                <p><strong>Время:</strong> {movie.Duration}</p>
                <p><strong>Из серии:</strong> {movie.Series}</p>
                </div>
            </div>
            <div class='movie-description'>
                <h2>Про что фильм «{movie.Title}»:</h2>
                <p>{movie.Description}</p>
            </div>
            <div class='video-container'>
                <video controls>
                    <source src='{movie.TrailerUrl}' type='video/mp4'>
                    Your browser does not support the video tag.
                </video>
            </div>
            <div class='buttons'>
                <div class='reviews-button'>Отзывы</div>
                <button class='bookmarks-button'>Добавить в закладки</button>
            </div>";
        

        html = html.Replace("<!-- Карточка фильма -->", movieHtml);

        return Html(html);
    }
    [Post("add-to-bookmarks")]
    public IHttpResponseResult AddToBookmarks([FromBody] JsonElement body)
    {
        // Получаем авторизованного пользователя
        var user = GetAuthorizedUser(Context);
        if (user == null)
        {
            return Json(new { message = "Вы не авторизованы!" });
        }

        // Извлекаем ID фильма из тела запроса
        if (!body.TryGetProperty("movieId", out var movieIdElement) || !int.TryParse(movieIdElement.ToString(), out var movieId))
        {
            return Json(new { message = "Неверный формат данных!" });
        }

        // Проверяем, существует ли фильм
        var movie = _context.GetById(movieId, "Movies");
        if (movie == null)
        {
            return Json(new { message = "Фильм не найден!" });
        }
        

        var existingFavorite = _favContext.FirstOrDefault(f => f.UserId == user.Id && f.MovieId == movieId);
        if (existingFavorite != null)
        {
            return Json(new { message = "Фильм уже добавлен в закладки!" });
        }

        // Добавляем запись в закладки
        var favorite = new Favorite
        {
            UserId = user.Id,
            MovieId = movieId
        };
        
        Console.WriteLine($"Adding favorite: UserId={favorite.UserId}, MovieId={favorite.MovieId}");
        try
        {
            Console.WriteLine($"SQL-запрос: INSERT INTO Favorites (UserId, MovieId) VALUES ({favorite.UserId}, {favorite.MovieId})");
            Console.WriteLine($"Adding favorite: UserId={favorite.UserId}, MovieId={favorite.MovieId}");
            _favContext.Create(favorite, "Favorites");
            Console.WriteLine("Фильм успешно добавлен в закладки");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении записи: {ex.Message}, Stack Trace: {ex.StackTrace}");
            throw;
        }

        return Json(new { message = "Фильм успешно добавлен в закладки!"});
    }
    private User? GetAuthorizedUser(HttpRequestContext context)
    {
        // Извлечение токена из куки
        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Name == "session-token");
        var token = cookie?.Value;

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Токен отсутствует в куки.");
            return null;
        }

        Console.WriteLine($"Извлеченный токен: {token}");

        // Проверка валидности токена
        if (!SessionStorage.ValidateToken(token))
        {
            Console.WriteLine("Токен невалиден.");
            return null;
        }

        // Получение ID пользователя, связанного с токеном
        var userId = SessionStorage.GetUserId(token);
        if (userId == null)
        {
            Console.WriteLine("UserId не найден для токена.");
            return null;
        }

        Console.WriteLine($"UserId для токена {token}: {userId}");
        return _userContext.GetById(int.Parse(userId), "Users");
    }
}