// ============================================================================
// BOUNDED CONTEXT: MonetizacionEcosistemaCreador
// RESPONSABILIDAD: Gestiona la monetización del ecosistema de creadores: elegibilidad del canal, configuración de productos de monetización (membresías, propinas/super thanks, anuncios), registro de ingresos, consulta de ganancias y pagos (payouts). Referencia channelId y videoId por UUID.
// Puerto: 5005  |  Base de datos MySQL: youtube_monetizacion
// ============================================================================
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MonetizacionEcosistemaCreador API",
        Version = "v1",
        Description = "Gestiona la monetización del ecosistema de creadores: elegibilidad del canal, configuración de productos de monetización (membresías, propinas/super thanks, anuncios), registro de ingresos, consulta de ganancias y pagos (payouts). Referencia channelId y videoId por UUID."
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
builder.Services.AddDbContext<MonetizacionEcosistemaCreadorDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

var app = builder.Build();

// Crea el esquema de la BD si no existe (alternativa simple a migraciones).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MonetizacionEcosistemaCreadorDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", () => Results.Ok(new { service = "MonetizacionEcosistemaCreador", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();

app.Run();
