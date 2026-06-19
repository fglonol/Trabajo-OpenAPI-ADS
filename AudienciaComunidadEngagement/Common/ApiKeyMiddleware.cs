namespace AudienciaComunidadEngagement.Common;

/// <summary>Valida la API Key (header X-API-Key) en cada request, salvo rutas públicas.</summary>
public class ApiKeyMiddleware
{
    public const string HeaderName = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsPublic(path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) ||
            string.IsNullOrWhiteSpace(provided))
        {
            await WriteError(context, StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED", $"Falta el header de API Key '{HeaderName}'.");
            return;
        }

        var expected = _configuration["ApiKey"];
        if (!string.Equals(provided, expected, StringComparison.Ordinal))
        {
            await WriteError(context, StatusCodes.Status401Unauthorized,
                "INVALID_API_KEY", "La API Key proporcionada no es válida.");
            return;
        }

        await _next(context);
    }

    private static bool IsPublic(string path)
        => path == "/"
           || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
           || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase);

    private static async Task WriteError(HttpContext context, int status, string code, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse(code, message));
    }
}
