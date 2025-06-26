using Microsoft.EntityFrameworkCore;
using LBAChamps.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LigaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC + Razor
builder.Services.AddControllersWithViews();
// ---------- CORS: permite que o Angular (http://localhost:4200) chame a API ----------
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("NgCors", p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LigaContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "Falha ao aplicar migrations");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// --------- habilita CORS antes da autorização ----------
app.UseCors("NgCors");

app.UseAuthorization();

app.MapStaticAssets();

// Rotas MVC (páginas)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ---------- expõe as rotas de API ----------
app.MapControllers();

app.Run();
