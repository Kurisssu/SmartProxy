using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    /// <summary>
    /// Reprezintă documentul principal gestionat de aplicație și extinde structura comună din MongoDB.
    /// </summary>
    public class Movie : MongoDocument
    {
        /// <summary>
        /// Titlul filmului; în MongoDB colecția va folosi câmpul acesta pentru afișare.
        /// </summary>
        public string name {  get; set; }

        /// <summary>
        /// Listă flexibilă pentru distribuție; se mapează direct într-un array BSON.
        /// </summary>
        public List<string> Actors { get; set; }

        /// <summary>
        /// Bugetul poate lipsi, de aceea folosim tipul decimal nullable.
        /// </summary>
        public decimal? Budget { get; set; }

        /// <summary>
        /// Text descriptiv ce ajută la scenarii de prezentare sau filtrare.
        /// </summary>
        public string Description { get; set; }
    }
}
