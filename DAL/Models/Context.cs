using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace DAL.Models
{
    public class Context
    {
        public IMongoCollection<User> Users; // коллекция в базе данных
        public IMongoCollection<Patient> Patients;
        public IMongoCollection<Test> Tests;
        public IMongoCollection<BsonDocument> TestsBson;
        public IMongoCollection<BsonDocument> NormsBson;
        public IGridFSBucket GridFs;   // файловое хранилище

        private IMongoDatabase database;
        public Context()
        {
            string connectionString = "mongodb://localhost:27017/MobilePsychoTest";
            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            database = client.GetDatabase(connection.DatabaseName);
            // получаем доступ к файловому хранилищу
            GridFs = new GridFSBucket(database);
            // обращаемся к коллекциям
            Users = database.GetCollection<User>("Users");
            Patients = database.GetCollection<Patient>("Patients");
            Tests = database.GetCollection<Test>("tests");
            TestsBson = database.GetCollection<BsonDocument>("tests");
            NormsBson = database.GetCollection<BsonDocument>("Norms");
        }
    }
}