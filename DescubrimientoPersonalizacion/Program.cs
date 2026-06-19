// ============================================================================
// BOUNDED CONTEXT: DescubrimientoPersonalizacion
// RESPONSABILIDAD: Hace descubrible y personalizado el contenido: búsqueda, feeds personalizados, contenido relacionado, tendencias, captura de señales de comportamiento e indexación. Guarda SOLO referencias y puntajes de ranking, no la metadata editorial completa (que vive en Catálogo).
// Puerto: 5003  |  Base de datos MySQL: youtube_descubrimiento
// ============================================================================
using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DescubrimientoPersonalizacion API",
        Version = "v1",
        Description = "Hace descubrible y personalizado el contenido: búsqueda, feeds personalizados, contenido relacionado, tendencias, captura de señales de comportamiento e indexación. Guarda SOLO referencias y puntajes de ranking, no la metadata editorial completa (que vive en Catálogo)."
    });

    var apiKeyScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = ApiKeyMiddleware.HeaderName,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "API Key requerida en el header X-API-Key.",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "ApiKey"
        }
    };
    options.AddSecurityDefinition("ApiKey", apiKeyScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { apiKeyScheme, Array.Empty<string>() }
    });
});

var connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<DescubrimientoPersonalizacionDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

var app = builder.Build();

// Crea el esquema de la BD si no existe (alternativa simple a migraciones).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DescubrimientoPersonalizacionDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => Results.Ok(new { service = "DescubrimientoPersonalizacion", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();

app.Run();
