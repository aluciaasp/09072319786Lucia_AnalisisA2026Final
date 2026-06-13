using EnviosRapidosGT.Api.Data;
using EnviosRapidosGT.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render expone el puerto en la variable de entorno PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- Servicios ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Envíos Rápidos GT API",
        Version = "v1",
        Description = "API REST para gestión y rastreo de envíos (Examen Final Análisis de Sistemas 2026)."
    });
});

// Base de datos SQLite (archivo envios.db en el directorio de la app).
var connString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=envios.db";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connString));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<ITarifaService, TarifaService>();
builder.Services.AddScoped<IEnvioService, EnvioService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Crea la base de datos al arrancar (suficiente para el prototipo).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ManejadorErroresMiddleware>();

// Swagger disponible también en producción para facilitar la evaluación.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Envíos Rápidos GT API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// Endpoint de salud / raíz informativa.
app.MapGet("/", () => Results.Ok(new
{
    servicio = "Envíos Rápidos GT API",
    estado = "OK",
    documentacion = "/swagger"
}));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();

app.Run();

// Necesario para exponer la clase Program a los tests de integración.
public partial class Program { }
