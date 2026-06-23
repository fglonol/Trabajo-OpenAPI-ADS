namespace PublicidadMarketplaceAnunciantes.Common;

/// <summary>Formato de error consistente entre los 6 bounded contexts.</summary>
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }

    public ErrorResponse() { }

    public ErrorResponse(string code, string message, object? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}
