namespace SyncNode.Settings
{
    /// <summary>
    /// Clasă de configurare care implementează interfața IMovieAPISettings.
    /// Conține lista de host-uri către care SyncNode distribuie evenimentele de sincronizare.
    /// 
    /// Configurarea se face în appsettings.json sub secțiunea "MovieAPISettings":
    /// {
    ///   "MovieAPISettings": {
    ///     "Hosts": [ "http://localhost:9001/api", "http://localhost:9002/api" ]
    ///   }
    /// }
    /// </summary>
    public class MovieAPISettings : IMovieAPISettings
    {
        /// <summary>
        /// Array de string-uri care conține URL-urile complete către instanțele MovieAPI.
        /// Fiecare URL trebuie să fie în formatul: http://host:port/api
        /// 
        /// Exemplu: [ "http://localhost:9001/api", "http://localhost:9002/api" ]
        /// 
        /// SyncNode va trimite evenimente de sincronizare către toate host-urile din această listă,
        /// cu excepția celui care este originea evenimentului (pentru a evita buclele).
        /// </summary>
        public string[] Hosts {  get; set; }
    }
}
