using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieAPI.Settings;

namespace MovieAPI.Repositores
{
    /// <summary>
    /// Implementare generică a repo-ului Mongo ce operează pe colecții deduse din tipul T.
    /// </summary>
    public class MongoRepository<T> : IMongoRepository<T> where T : MongoDocument
    {
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDbSettings dbSettings)
        {
            _db = new MongoClient(dbSettings.ConnectionString).GetDatabase(dbSettings.DatabaseName);

            // Numele colecției este numele clasei scris cu litere mici (ex: "movie").
            string tableName = typeof(T).Name.ToLower();
            _collection = _db.GetCollection<T>(tableName);
        }

        public void DeleteRecord(Guid id)
        {
            _collection.DeleteOne(doc => doc.Id == id);
        }

        public List<T> GetAllRecords()
        {
            var records = _collection.Find(new BsonDocument()).ToList();

            return records;
        }

        public T GetRecordById(Guid id)
        {
            var record = _collection.Find(doc => doc.Id == id).FirstOrDefault();

            return record;
        }

        public T InsertRecord(T record)
        {
            _collection.InsertOne(record);

            return record;
        }

        public void UpsertRecord(T record)
        {
            // ReplaceOne cu IsUpsert = true asigură inserare sau actualizare în funcție de existență.
            _collection.ReplaceOne(doc => doc.Id == record.Id, record, new ReplaceOptions() { IsUpsert = true });
        }
    }
}