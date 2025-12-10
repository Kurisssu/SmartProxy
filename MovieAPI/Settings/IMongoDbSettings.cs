namespace MovieAPI.Settings
{
    /// <summary>
    /// Interfață folosită pentru a injecta setările MongoDB prin <see cref="IOptions{T}"/>.
    /// </summary>
    public interface IMongoDbSettings
    {
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
    }
}
