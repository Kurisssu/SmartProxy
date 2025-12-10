using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SyncNode.Services;
using SyncNode.Settings;

/// <summary>
/// SyncNode este serviciul centralizat de sincronizare care primește evenimente de modificare
/// de la instanțele MovieAPI și le distribuie periodic către toate instanțele pentru a menține
/// consistența datelor într-un sistem distribuit.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Configurăm serializarea GUID-urilor pentru MongoDB folosind reprezentarea standard
// Acest lucru asigură compatibilitatea între diferite instanțe și evită probleme de serializare
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Adăugăm suport pentru Razor Pages (pentru paginile de eroare)
builder.Services.AddRazorPages();

// Adăugăm suport pentru API Controllers (pentru endpoint-ul /sync)
builder.Services.AddControllers();

// Configurăm setările pentru MovieAPI din secțiunea MovieAPISettings din appsettings.json
// Aceste setări conțin lista de host-uri către care trebuie distribuite evenimentele
builder.Services.Configure<MovieAPISettings>(builder.Configuration.GetSection("MovieAPISettings"));

// Înregistrăm interfața IMovieAPISettings ca singleton pentru a fi disponibilă în toată aplicația
// Folosim IOptions pattern pentru a accesa configurația tipărită
builder.Services.AddSingleton<IMovieAPISettings>(provider => provider.GetRequiredService<IOptions<MovieAPISettings>>().Value);

// Înregistrăm SyncWorkJobService ca singleton - acesta gestionează colecția de evenimente
// și procesarea periodică a acestora
builder.Services.AddSingleton<SyncWorkJobService>();

// Înregistrăm SyncWorkJobService ca Hosted Service - acest lucru permite rularea sa
// în background ca serviciu de lungă durată care procesează evenimentele la fiecare 15 secunde
builder.Services.AddHostedService(provider => provider.GetService<SyncWorkJobService>());

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

// Mapează rutele pentru API Controllers - asigură că endpoint-ul /sync este accesibil
// Acest endpoint primește evenimente de sincronizare de la instanțele MovieAPI
app.MapControllers();

// Pornește serverul web și începe să asculte cereri
app.Run();
