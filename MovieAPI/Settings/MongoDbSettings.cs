namespace MovieAPI.Settings
{
    /// <summary>
    /// Valori din config pentru conectarea la instanța MongoDB a fiecărei aplicații.
    /// </summary>
    public class MongoDbSettings : IMongoDbSettings
    {
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
    }
}
