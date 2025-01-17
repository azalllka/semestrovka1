using HttpServerLibrary;
using HttpServerLibrary.HttpResponse;

public class RedirectResult : IHttpResponseResult
{
    private readonly string _location;

    public RedirectResult(string location)
    {
        _location = location;
    }

    public void Execute(HttpRequestContext context)
    {
        // Set the HTTP status code to 302 (Found) for a redirect
        context.Response.StatusCode = 302;

        // Set the Location header to the URL to redirect to
        context.Response.Headers.Add("Location", _location);

        // End the response (no content body for redirect)
        context.Response.Close();
    }
}