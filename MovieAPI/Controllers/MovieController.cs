using Common.Models;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Repositores;
using MovieAPI.Services;

namespace MovieAPI.Controllers
{
    /// <summary>
    /// Controller-ul REST care expune CRUD pentru filme și endpoint-uri speciale pentru sincronizare.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMongoRepository<Movie> _movieRepository;
        private readonly ISyncService<Movie> _movieSyncService;

        public MovieController(IMongoRepository<Movie> movieRepository, ISyncService<Movie> movieSyncService)
        {
            _movieRepository = movieRepository;
            _movieSyncService = movieSyncService;
        }

        [HttpGet]
        public ActionResult<List<Movie>> GetAllMovies()
        {
            var records = _movieRepository.GetAllRecords();

            return records;
        }

        [HttpGet("{id}")]
        public Movie GetMovieById(Guid id)
        {
            var result = _movieRepository.GetRecordById(id);

            return result;
        }

        [HttpPost]
        public IActionResult Create(Movie movie)
        {
            movie.LastChangedAt = DateTime.UtcNow; // setăm timestamp-ul înainte de persistare.
            var result = _movieRepository.InsertRecord(movie);

            _movieSyncService.Upsert(movie); // notificăm SyncNode pentru replicare.

            return Ok(result);
        }

        [HttpPut]
        public IActionResult Upsert(Movie movie)
        {
            if (movie.Id == Guid.Empty)
            {
                return BadRequest("Empty Id");
            }

            movie.LastChangedAt = DateTime.UtcNow;
            _movieRepository.UpsertRecord(movie);

            _movieSyncService.Upsert(movie); // aceleași date sunt trimise și către parteneri.

            return Ok(movie);
        }

        /// <summary>
        /// Endpoint folosit exclusiv de SyncNode; aplică regulile LWW.
        /// </summary>
        [HttpPut("sync")]
        public IActionResult UpsertSync(Movie movie)
        {
            var existingMovie = _movieRepository.GetRecordById(movie.Id);

            if (existingMovie == null || movie.LastChangedAt > existingMovie.LastChangedAt)
            {
                _movieRepository.UpsertRecord(movie);
            }

            return Ok();
        }

        /// <summary>
        /// Similar cu UpsertSync dar pentru operația de ștergere propagată.
        /// </summary>
        [HttpDelete("sync")]
        public IActionResult DeleteSync(Movie movie)
        {
            var existingMovie = _movieRepository.GetRecordById(movie.Id);

            if (existingMovie != null || movie.LastChangedAt > existingMovie.LastChangedAt)
            {
                _movieRepository.DeleteRecord(movie.Id);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var movie = _movieRepository.GetRecordById(id);

            if (movie == null)
            {
                return BadRequest("Movie doesn't exist");
            }

            _movieRepository.DeleteRecord(id);

            movie.LastChangedAt = DateTime.UtcNow;
            _movieSyncService.Delete(movie); // trimitem pachetul de ștergere către SyncNode.

            return Ok("Deleted " + id);
        }
    }
}
