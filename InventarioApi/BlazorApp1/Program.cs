using InventarioAPI.Repository;
using InventarioAPI.Repository.Implementacion;
using InventarioAPI.Repository.Interfaz;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SistemaPos",
        Version = "v1"
    });
});

// 3. Inyección de Dependencias
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();

// 4. Base de Datos (SQL Server en Podman/Local)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

var app = builder.Build();

// 5. Configuración del Middleware para Desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventarioPOS API v1");
        c.RoutePrefix = "swagger"; // Acceso directo en http://localhost:PORT/swagger
    });
}
else
{
    // Solo forzar HTTPS en producción para evitar bloqueos SSL locales en Linux
    app.UseHttpsRedirection();
}

// 6. Pipeline de solicitudes HTTP (El orden importa)
app.UseCors("AllowDev");
app.UseAuthorization();
app.MapControllers();

app.Run();