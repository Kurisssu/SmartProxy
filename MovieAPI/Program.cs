using Common.Models;
using Microsoft.Extensions.Options;
using MovieAPI.Repositores;
using MovieAPI.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MovieAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrăm serializatorul Guid pentru a evita inconsistențe între instanțe (Standard = format recomandat).
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Adăugăm atât Razor Pages (pentru UI minimal) cât și infrastructura HTTP.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor(); // necesar pentru a obține host-ul curent în SyncService.

// Injectăm setările MongoDB și SyncService direct din appsettings.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoDbSettings>(provider => provider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.Configure<SyncServiceSettings>(builder.Configuration.GetSection("SyncServiceSettings"));
builder.Services.AddSingleton<ISyncServiceSettings>(provider => provider.GetRequiredService<IOptions<SyncServiceSettings>>().Value);

// Controller-ele API sunt necesare pentru rutele /api/movie, pe lângă paginile Razor.
builder.Services.AddControllers();

// Repozitoriu Mongo generic + serviciul de sincronizare pentru tipul Movie.
builder.Services.AddScoped<IMongoRepository<Movie>, MongoRepository<Movie>>();
builder.Services.AddScoped<ISyncService<Movie>, SyncService<Movie>>();

var app = builder.Build();

// Pipeline HTTP standard; în producție maschează erorile prin /Error.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization(); // nu există politici, dar middleware-ul rămâne pentru conformitate.

// Mapează atât API-ul (JSON) cât și partea de UI.
app.MapControllers();
app.MapRazorPages();

app.Run();
