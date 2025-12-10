using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    /// <summary>
    /// Mesaj standard folosit între MovieAPI și SyncNode pentru propagarea modificărilor.
    /// </summary>
    public class SyncEntity
    {
        /// <summary>
        /// Id-ul documentului sincronizat; dicționarul din SyncNode îl folosește drept cheie.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Marcajul temporal al modificării, folosit pentru scenariul „last write wins”.
        /// </summary>
        public DateTime LastChangedAt { get; set; }

        /// <summary>
        /// Payload-ul serializat al obiectului (JSON original al Movie-ului).
        /// </summary>
        public string JsonData { get; set; }

        /// <summary>
        /// Metoda HTTP care trebuie executată downstream (PUT/DELETE).
        /// </summary>
        public string SyncType { get; set; }

        /// <summary>
        /// Tipul concret al obiectului (ex: Movie) pentru construirea endpoint-ului.
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Originea mesajului (host:port) pentru a evita buclele de sincronizare.
        /// </summary>
        public string Origin { get; set; }
    }
}
