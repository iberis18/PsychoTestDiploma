using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DAL.Models
{
    public class Patient
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string name { get; set; }
        public string token { get; set; }
        public List<string> tests { get; set; }
        public List<PatientResult>? results { get; set; }
    }
}
