using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    /// <summary>
    /// Clasă de bază pentru toate documentele persistate în MongoDB, asigură câmpurile comune.
    /// </summary>
    public abstract class MongoDocument
    {
        /// <summary>
        /// Identificatorul din Mongo; atributul <see cref="BsonIdAttribute"/> indică faptul că este cheia primară.
        /// </summary>
        [BsonId]
        public Guid Id { get; set; }

        /// <summary>
        /// Timpul ultimei modificări UTC, folosit pentru rezolvarea conflictelor la sincronizare.
        /// </summary>
        public DateTime LastChangedAt { get; set; }
    }
}
