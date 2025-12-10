using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    /// <summary>
    /// Helper simplu pentru trimiterea request-urilor JSON către serviciile interne (de ex. SyncNode).
    /// Folosește un <see cref="HttpClient"/> static pentru a reutiliza conexiunile.
    /// </summary>
    public class HttpClientUtility
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Creează și trimite o cerere HTTP cu payload JSON și metoda specificată.
        /// </summary>
        public static HttpResponseMessage SendJson(string json, string url, string method)
        {
            // Normalizăm metoda (PUT/DELETE/etc.) și pregătim conținutul.
            var httpMethod = new HttpMethod(method.ToUpper());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Request-ul este construit explicit pentru a putea controla ulterior headerele dacă e nevoie.
            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = content
            };

            // În contextul laboratorului blocăm thread-ul, însă în aplicații reale e de preferat async/await.
            var task = _client.SendAsync(request);
            task.Wait();

            return task.Result;
        }
    }
}