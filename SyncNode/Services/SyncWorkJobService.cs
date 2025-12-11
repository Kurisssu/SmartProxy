using Common.Models;
using Common.Utilities;
using SyncNode.Settings;
using System.Collections.Concurrent;

namespace SyncNode.Services
{
    /// <summary>
    /// SyncWorkJobService este un serviciu de lungă durată (Hosted Service) care gestionează
    /// sincronizarea datelor între multiple instanțe MovieAPI. Funcționează ca un hub central
    /// care primește evenimente de modificare și le distribuie periodic către toate instanțele.
    /// </summary>
    public class SyncWorkJobService : IHostedService
    {
        /// <summary>
        /// Colecție thread-safe care stochează evenimentele de sincronizare în așteptare.
        /// Folosim ConcurrentDictionary pentru a permite acces concurent sigur din multiple thread-uri.
        /// Cheia este Id-ul documentului, iar valoarea este SyncEntity-ul cu toate detaliile.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, SyncEntity> documents = new ConcurrentDictionary<Guid, SyncEntity>();

        /// <summary>
        /// Setările care conțin lista de host-uri către care trebuie distribuite evenimentele.
        /// Acestea sunt configurate în appsettings.json sub secțiunea MovieAPISettings.
        /// </summary>
        private readonly IMovieAPISettings _settings;

        /// <summary>
        /// Timer-ul care declanșează procesarea periodică a evenimentelor.
        /// La fiecare 15 secunde, metoda DoSendWork este apelată pentru a distribui evenimentele.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Constructor care primește setările prin dependency injection.
        /// </summary>
        public SyncWorkJobService(IMovieAPISettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Adaugă un eveniment de sincronizare în colecția thread-safe.
        /// 
        /// Logica de adăugare:
        /// - Dacă nu există un eveniment pentru acest Id, se adaugă direct
        /// - Dacă există deja un eveniment, se compară timestamp-urile (LastChangedAt)
        /// - Se păstrează doar evenimentul cu timestamp-ul mai recent (strategia "last write wins")
        /// 
        /// Această abordare asigură că în cazul în care sosesc multiple evenimente pentru același
        /// document într-un interval scurt, se procesează doar cel mai recent.
        /// </summary>
        public void AddItem(SyncEntity entity)
        {
            SyncEntity document = null;
            // Verificăm dacă există deja un eveniment pentru acest Id
            bool isPresent = documents.TryGetValue(entity.Id, out document);

            // Adăugăm sau actualizăm evenimentul doar dacă:
            // 1. Nu există deja un eveniment pentru acest Id, SAU
            // 2. Există un eveniment, dar noul eveniment are un timestamp mai recent
            if (!isPresent || (isPresent && entity.LastChangedAt > document.LastChangedAt))
            {
                documents[entity.Id] = entity;
            }
        }

        /// <summary>
        /// Metodă apelată când serviciul este pornit. Inițializează timer-ul care va declanșa
        /// procesarea periodică a evenimentelor la fiecare 15 secunde.
        /// 
        /// TimeSpan.Zero înseamnă că prima execuție se face imediat după pornire.
        /// TimeSpan.FromSeconds(15) înseamnă că apoi se execută la fiecare 15 secunde.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoSendWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Metodă apelată când serviciul este oprit. Oprește timer-ul pentru a preveni
        /// procesarea evenimentelor după oprirea aplicației.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Schimbăm timer-ul să nu mai fie activ (Timeout.Infinite înseamnă niciodată)
            _timer?.Change(Timeout.Infinite, 0);
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Metodă apelată periodic de către timer pentru a procesa și distribui evenimentele.
        /// 
        /// Fluxul de lucru:
        /// 1. Iterează prin toate evenimentele din colecție
        /// 2. Extrage fiecare eveniment din colecție (TryRemove asigură thread-safety)
        /// 3. Identifică toate instanțele destinație (exclude originea pentru a evita bucle)
        /// 4. Trimite cererea HTTP către fiecare instanță destinație folosind metoda specificată
        /// 
        /// Construcția URL-ului: {receiver}/{ObjectType}/sync
        /// Exemplu: http://localhost:9001/api/Movie/sync
        /// 
        /// Metoda HTTP folosită este cea specificată în SyncType (PUT pentru upsert, DELETE pentru ștergere).
        /// </summary>
        private void DoSendWork(object state)
        {
            // Iterăm prin toate evenimentele din colecție
            foreach (var doc in documents)
            {
                SyncEntity entity = null;
                // Extragem evenimentul din colecție - TryRemove este atomic și thread-safe
                // După extragere, evenimentul nu mai este în colecție, deci nu va fi procesat din nou
                var isPresent = documents.TryRemove(doc.Key, out entity);

                if (isPresent)
                {
                    // Filtrăm lista de host-uri pentru a exclude originea evenimentului
                    // Acest lucru previne buclele de sincronizare (nu trimitem înapoi către sursă)
                    // Exemplu: dacă evenimentul vine de la localhost:9001, nu îl trimitem înapoi acolo
                    var receivers = _settings.Hosts.Where(x => !x.Contains(entity.Origin));

                    // Trimitem evenimentul către fiecare instanță destinație
                    foreach(var receiver in receivers)
                    {
                        // Construim URL-ul complet către endpoint-ul de sincronizare
                        // Format: http://localhost:9001/api/Movie/sync
                        var url = $"{receiver}/{entity.ObjectType}/sync";

                        try
                        {
                            // Trimitem cererea HTTP către instanța destinație
                            // entity.JsonData conține datele serializate ale obiectului
                            // entity.SyncType specifică metoda HTTP (PUT sau DELETE)
                            var result = HttpClientUtility.SendJson(entity.JsonData, url, entity.SyncType);

                            // Verificăm dacă cererea a reușit
                            // Notă: În implementarea actuală, erorile nu sunt loggate sau procesate
                            // Într-o versiune de producție, ar trebui implementat retry logic și logging
                            if (!result.IsSuccessStatusCode)
                            {
                                // Aici ar putea fi adăugată logica de retry sau logging pentru erori
                            }
                        }
                        catch (Exception ex)
                        {
                            // Capturăm excepțiile pentru a preveni oprirea procesării altor evenimente
                            // Într-o versiune de producție, ar trebui loggate pentru debugging
                            // Notă: În implementarea actuală, excepțiile sunt ignorate silențios
                        }
                    }
                }
            }
        }
    }

}
