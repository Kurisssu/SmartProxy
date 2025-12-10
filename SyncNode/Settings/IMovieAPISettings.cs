namespace SyncNode.Settings
{
    /// <summary>
    /// Interfață care definește contractul pentru setările de configurare ale MovieAPI.
    /// Această interfață este folosită pentru dependency injection și permite testarea
    /// și înlocuirea configurației în scenarii de testare.
    /// 
    /// Pattern-ul folosit este Options Pattern din .NET, care permite configurarea
    /// tipărită și validată a setărilor aplicației.
    /// </summary>
    public interface IMovieAPISettings
    {
        /// <summary>
        /// Proprietate care conține lista de host-uri către care trebuie distribuite
        /// evenimentele de sincronizare. Fiecare element reprezintă URL-ul complet
        /// către o instanță MovieAPI (ex: "http://localhost:9001/api").
        /// </summary>
        public string[] Hosts { get; set; }
    }
}
