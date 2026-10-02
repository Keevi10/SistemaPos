using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

// Middleware con CSP actualizado para permitir CDNs en connect-src y scripts
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.tailwindcss.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data: blob: https:; " +
        "connect-src 'self' ws: wss: http://localhost:* https://localhost:* https://cdn.jsdelivr.net https://cdn.tailwindcss.com; " +
        "form-action 'self';");
    await next();
});

var defaultCulture = new CultureInfo("es-MX");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Pos/Error");
    app.UseHsts();
    app.UseHttpsRedirection(); // Solo forzar HTTPS en Producción
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pos}/{action=Index}/{id?}");

app.Run();