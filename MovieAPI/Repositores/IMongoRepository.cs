using Common.Models;

namespace MovieAPI.Repositores
{
    /// <summary>
    /// Contract generic pentru operații CRUD pe colecțiile MongoDB.
    /// </summary>
    public interface IMongoRepository<T> where T: MongoDocument
    {
        List<T> GetAllRecords();
        T InsertRecord(T record);
        T GetRecordById(Guid id);
        void UpsertRecord(T record);
        void DeleteRecord(Guid id);
    }
}
