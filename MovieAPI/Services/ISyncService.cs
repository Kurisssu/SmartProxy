using Common.Models;

namespace MovieAPI.Services
{
    /// <summary>
    /// Interfață care descrie acțiunile de sincronizare ce trebuie executate după CRUD.
    /// </summary>
    public interface ISyncService<T> where T: MongoDocument
    {
        HttpResponseMessage Upsert(T record);
        HttpResponseMessage Delete(T record);
    }
}