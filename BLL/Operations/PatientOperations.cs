using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Interfaces;
using DAL.Interfaces;
using BLL.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace BLL.Operations
{
    public class PatientOperations : IPatient
    {
        IUnitOfWork db; //репозиторий

        public PatientOperations(IUnitOfWork db)
        {
            this.db = db;
        }


        public async Task<IEnumerable<Patient>> GetPatients()
        {
            return (await db.Patients.GetAll()).Select(i => new Patient(i)).ToList();
        }

        public async Task<Patient> GetPatientById(string id)
        {
            return new Patient(await db.Patients.GetItemById(id));
        }

        public async Task<Patient> GetPatientByToken(string token)
        {
            DAL.Models.Patient p = await db.Patients.GetIdentity(token, null);
            if (p != null)
                return new Patient(p);
            else return null;
        }

        public async Task<double> GetPatientsPagesCount()
        {
            return Math.Ceiling((double)(await db.Patients.ItemsCount()) / 10.0);
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithCount(int pageNumber)
        {
            List<Patient> allPatients = (await db.Patients.GetAll()).Select(i => new Patient(i)).ToList();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= allPatients.Count)
                count = allPatients.Count - start;
            Patient[] page = new Patient[count];
            allPatients.CopyTo(start, page, 0, count);
            return page;
        }

        public async Task<IEnumerable<Patient>> GetPatientsByName(string value)
        {
            return (await db.Patients.GetAll())
                .ToList()
                .FindAll(x => x.name.ToLower().Contains(value.ToLower()) == true)
                .Select(x => new Patient(x));
        }

        public async Task<double> GetPatientsByNamePagesCount(string value)
        {
            return Math.Ceiling((double)((await GetPatientsByName(value)).Count()) / 10.0);
        }

        public async Task<IEnumerable<Patient>> GetPatientsByNameWithCount(int pageNumber, string name)
        {
            List<Patient> patients = (await GetPatientsByName(name)).ToList();
            int start = (pageNumber - 1) * 10;
            int count = 10;
            if (start + count >= patients.Count)
                count = patients.Count - start;
            Patient[] page = new Patient[count];
            patients.CopyTo(start, page, 0, count);
            return page;
        }

        public async Task<Patient> GetPatientsResultsByTestId(string patientId, string testId)
        {
            Patient patient = await GetPatientById(patientId);
            patient.Results = patient.Results.Where(i => i.Test == testId).ToList();
            return patient;
        }

        //получаем все назначенные пациенту тесты 
        public async Task<IEnumerable<Test>> GetTestsByPatientToken(Patient patient)
        {
            List<Test> documents = (await db.Tests.GetAll()).Select(i => new Test(i)).ToList();
            List<Test> tests = new List<Test>();

            if (patient.Tests != null)
                foreach (string idTest in patient.Tests)
                    foreach (var doc in documents)
                        if (idTest == doc.Id)
                            tests.Add(doc);
            return tests;
        }


        public async Task<string> CreatePatient(Patient p)
        {
            string token = new TokenOperations().GenerateToken();

            await db.Patients.Create(new DAL.Models.Patient()
            {
                id = p.Id, 
                name = p.Name,
                tests = p.Tests, 
                token = token,
                results = p.Results.Select(r => new DAL.Models.PatientResult()
                {
                    comment = r.Comment,
                    date = r.Date,
                    test = r.Test,
                    scales = r.Scales.Select(s => new DAL.Models.PatientResult.Scale()
                    {
                        idNormScale = s.IdNormScale,
                        idTestScale = s.IdTestScale,
                        gradationNumber = s.GradationNumber,
                        interpretation = s.Interpretation,
                        name = s.Name,
                        scores = s.Scores
                    }).ToList()
                }).ToList()
            });

            return token;
        }

        public async Task UpdatePatient(Patient p)
        {
            await db.Patients.Update(new DAL.Models.Patient()
            {
                id = p.Id,
                name = p.Name,
                tests = p.Tests,
                token = p.Token,
                results = p.Results.Select(r => new DAL.Models.PatientResult()
                {
                    comment = r.Comment,
                    date = r.Date,
                    test = r.Test,
                    scales = r.Scales.Select(s => new DAL.Models.PatientResult.Scale()
                    {
                        idNormScale = s.IdNormScale,
                        idTestScale = s.IdTestScale,
                        gradationNumber = s.GradationNumber,
                        interpretation = s.Interpretation,
                        name = s.Name,
                        scores = s.Scores
                    }).ToList()
                }).ToList()
            });
        }

        public async Task RemovePatient(string id)
        {
            await db.Patients.Remove(id);
        }
    }
}