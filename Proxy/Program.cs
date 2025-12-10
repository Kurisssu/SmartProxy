using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;

/// <summary>
/// Aplicația Proxy funcționează ca un API Gateway inteligent care distribuie traficul
/// către multiple instanțe MovieAPI, oferind load balancing și caching pentru optimizarea performanței.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Adăugăm suport pentru Razor Pages (pentru paginile de eroare și interfața web de bază)
builder.Services.AddRazorPages();

// Configurăm Ocelot să încarce configurația din ocelot.json
// SetBasePath asigură că Ocelot găsește fișierul de configurare în directorul corect
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddOcelot();

// Înregistrăm Ocelot în containerul de dependențe și activăm CacheManager
// CacheManager folosește un dicționar în memorie pentru stocarea temporară a răspunsurilor
// Acest lucru permite cache-ul pentru cererile GET conform configurației din ocelot.json
builder.Services.AddOcelot(builder.Configuration).AddCacheManager(x => x.WithDictionaryHandle());

var app = builder.Build();

// Configurarea pipeline-ului de procesare a cererilor HTTP
if (!app.Environment.IsDevelopment())
{
    // În producție, redirecționăm erorile către o pagină dedicată
    app.UseExceptionHandler("/Error");
}

// Permitem servirea fișierelor statice (CSS, JS, imagini) din folderul wwwroot
app.UseStaticFiles();

// Activăm routing-ul pentru a direcționa cererile către handler-ele corespunzătoare
app.UseRouting();

// Activăm autorizarea (deși în acest proiect nu este implementată autentificare)
app.UseAuthorization();

// Mapează rutele pentru Razor Pages
app.MapRazorPages();

// Inițializăm middleware-ul Ocelot care procesează toate cererile HTTP
// Wait() este necesar deoarece UseOcelot() returnează un Task
// Ocelot va intercepta cererile, le va procesa conform ocelot.json și le va ruta către instanțele MovieAPI
app.UseOcelot().Wait();

// Pornește serverul web și începe să asculte cereri
app.Run();