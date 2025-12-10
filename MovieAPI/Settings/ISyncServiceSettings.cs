namespace MovieAPI.Settings
{
    /// <summary>
    /// Descrie parametrii de configurare necesari serviciului de sincronizare.
    /// </summary>
    public interface ISyncServiceSettings
    {
        public string Host { get; set; }
        public string UpsertHttpMethod { get; set; }
        public string DeleteHttpMethod { get; set; }
    }
}
