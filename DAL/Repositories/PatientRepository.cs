using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Interfaces;
using DAL.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class PatientRepository : IRepository<Patient>
    {
        private Context db;

        public PatientRepository(Context dbcontext)
        {
            this.db = dbcontext;
        }

        //список всех пациентов
        public async Task<IEnumerable<Patient>> GetAll()
        {
            var builder = new MongoDB.Driver.FilterDefinitionBuilder<Patient>();
            var filter = builder.Empty;
            return await db.Patients.Find(filter).ToListAsync();
        }

        //получаем пациента по токену
        public async Task<Patient> GetIdentity(string token, string value)
        {
            return await db.Patients.Find(new BsonDocument("token", token)).FirstOrDefaultAsync();
        }

        public async Task<long> ItemsCount()
        {
            var builder = new FilterDefinitionBuilder<Patient>();
            var filter = builder.Empty;
            return await db.Patients.CountDocumentsAsync(filter);
        }

        //получаем количество страниц с пациентами, если на странице 10 пациентов
        //public async Task<double> GetPagesCount()
        //{
        //    var builder = new FilterDefinitionBuilder<Patient>();
        //    var filter = builder.Empty;
        //    long count = await db.Patients.CountDocumentsAsync(filter);
        //    return Math.Ceiling((double)count / 10.0);
        //}

        //получаем часть пациентов для пагинации
        //public async Task<IEnumerable<Patient>> GetWithCount(int pageNumber)
        //{
        //    var builder = new FilterDefinitionBuilder<Patient>();
        //    var filter = builder.Empty;
        //    List<Patient> allPatients = await db.Patients.Find(filter).ToListAsync();
        //    int start = (pageNumber - 1) * 10;
        //    int count = 10;
        //    if (start + count >= allPatients.Count)
        //        count = allPatients.Count - start;
        //    Patient[] page = new Patient[count];
        //    allPatients.CopyTo(start, page, 0, count);
        //    return page;
        //}

        //фильтрация по имени
        //public async Task<IEnumerable<Patient>> GetByName(string value)
        //{
        //    var builder = new FilterDefinitionBuilder<Patient>();
        //    var filter = builder.Empty;
        //    var allPatients = await db.Patients.Find(filter).ToListAsync();
        //    return allPatients.FindAll(x => x.name.ToLower().Contains(value.ToLower()) == true);
        //}

        //получаем количество страниц с пациентами c фильтрацией по имени, если на странице 10 пациентов
        //public async Task<double> GetByNamePagesCount(string value)
        //{
        //    var patients = await GetByName(value);
        //    patients = patients.ToList();
        //    long count = patients.Count();
        //    return Math.Ceiling((double)count / 10.0);
        //}

        //получаем часть пациентов c фильтрацией по имени для пагинации
        //public async Task<IEnumerable<Patient>> GetByNameWithCount(int pageNumber, string name)
        //{
        //    List<Patient> patients = new List<Patient>();
        //    var p = await GetByName(name);
        //    patients = p.ToList();
        //    int start = (pageNumber - 1) * 10;
        //    int count = 10;
        //    if (start + count >= patients.Count)
        //        count = patients.Count - start;
        //    Patient[] page = new Patient[count];
        //    patients.CopyTo(start, page, 0, count);
        //    return page;
        //}

        //фильтрация результатов для статистики по id теста
        //public async Task<Patient> GetPatientsResultsByTestId(string patientId, string testId)
        //{
        //    Patient patient = await GetItemById(patientId);
        //    List<PatientResult> r = new List<PatientResult>();
        //    foreach (PatientResult result in patient.results)
        //        if (result.test == testId)
        //            r.Add(result);
        //    patient.results = r;
        //    return patient;
        //}

        //получаем пациента по id
        public async Task<Patient> GetItemById(string id)
        {
            return await db.Patients.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }


        // добавление пациента
        public async Task Create(Patient item)
        {
            await db.Patients.InsertOneAsync(item);
        }

        // обновление пациента
        public async Task Update(Patient item)
        {
            BsonDocument doc = new BsonDocument("_id", new ObjectId(item.id));
            Patient patient = await db.Patients.Find(new BsonDocument("_id", new ObjectId(item.id))).FirstOrDefaultAsync();
            if (item.token == null)
                item.token = patient.token;
            await db.Patients.ReplaceOneAsync(doc, item);
        }

        // удаление пациента
        public async Task Remove(string id)
        {
            await db.Patients.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }

    }
}