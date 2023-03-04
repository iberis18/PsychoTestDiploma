using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DAL.Models
{
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string name { get; set; }
    }
}