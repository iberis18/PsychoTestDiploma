using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DAL.Models
{
    public class Test
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string instruction { get; set; }
    }
}