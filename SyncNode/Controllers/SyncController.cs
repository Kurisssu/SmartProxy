using Common.Models;
using Microsoft.AspNetCore.Mvc;
using SyncNode.Services;

namespace SyncNode.Controllers
{
    /// <summary>
    /// Controller-ul SyncController expune endpoint-ul principal pentru primirea evenimentelor
    /// de sincronizare de la instanțele MovieAPI. Aceste evenimente sunt apoi procesate
    /// asincron de către SyncWorkJobService și distribuite către toate instanțele.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        /// <summary>
        /// Referință către serviciul de procesare a evenimentelor de sincronizare.
        /// Acest serviciu gestionează colecția thread-safe de evenimente și distribuția periodică.
        /// </summary>
        private readonly SyncWorkJobService _workJobService;

        /// <summary>
        /// Constructor care primește SyncWorkJobService prin dependency injection.
        /// </summary>
        public SyncController(SyncWorkJobService workJobService)
        {
            _workJobService = workJobService;
        }

        /// <summary>
        /// Endpoint POST /sync care primește evenimente de sincronizare de la instanțele MovieAPI.
        /// 
        /// Fluxul de lucru:
        /// 1. MovieAPI trimite un SyncEntity când apare o modificare (creare, actualizare, ștergere)
        /// 2. SyncController adaugă evenimentul în buffer-ul thread-safe din SyncWorkJobService
        /// 3. SyncWorkJobService procesează periodic (la 15 secunde) evenimentele și le distribuie
        /// 
        /// Parametrul entity conține:
        /// - Id: identificatorul documentului modificat
        /// - LastChangedAt: timestamp-ul modificării (pentru rezolvarea conflictelor)
        /// - JsonData: datele serializate ale obiectului
        /// - SyncType: metoda HTTP (PUT pentru upsert, DELETE pentru ștergere)
        /// - ObjectType: tipul obiectului (ex: "Movie")
        /// - Origin: host-ul de origine (pentru a evita buclele de sincronizare)
        /// </summary>
        [HttpPost]
        public ActionResult<SyncEntity> Sync(SyncEntity entity)
        {
            // Adăugăm evenimentul în colecția thread-safe din SyncWorkJobService
            // Metoda AddItem verifică automat dacă evenimentul este mai recent decât unul existent
            // folosind timestamp-ul LastChangedAt (strategia "last write wins")
            _workJobService.AddItem(entity);

            // Returnăm OK imediat - procesarea reală se face asincron de către SyncWorkJobService
            // Acest pattern permite un răspuns rapid către MovieAPI și procesare în background
            return Ok();
        }
    }
}
