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
    public class DBPatientOperation : IDBPatient
    {
        IUnitOfWork db; //репозиторий

        public DBPatientOperation(IUnitOfWork db)
        {
            this.db = db;
        }


        public async Task<IEnumerable<Patient>> GetPatients()
        {
            var patients = await db.Patients.GetAll();
            return patients.Select(i => new Patient(i)).ToList();
        }

        public async Task<Patient> GetPatientById(string id)
        {
            return new Patient(await db.Patients.GetItemById(id));
        }

        public async Task<double> GetPatientsPagesCount()
        {
            return await db.Patients.GetPagesCount();
        }

        public async Task<IEnumerable<Patient>> GetPatientsWithCount(int pageNumber)
        {
            var patients = await db.Patients.GetWithCount(pageNumber);
            return patients.Select(i => new Patient(i)).ToList();
        }

        public async Task<IEnumerable<Patient>> GetPatientsByName(string value)
        {
            var patients = await db.Patients.GetByName(value);
            return patients.Select(i => new Patient(i)).ToList();
        }
        public async Task<double> GetPatientsByNamePagesCount(string value)
        {
            return await db.Patients.GetByNamePagesCount(value);
        }

        public async Task<IEnumerable<Patient>> GetPatientsByNameWithCount(int pageNumber, string name)
        {
            var patients = await db.Patients.GetByNameWithCount(pageNumber, name);
            return patients.Select(i => new Patient(i)).ToList();
        }
        public async Task<Patient> GetPatientsResultsByTestId(string patientId, string testId)
        {
            Patient patient = await GetPatientById(patientId);
            patient.Results = patient.Results.Where(i => i.Test == testId).ToList();
            return patient;
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
            await db.Patients.Create(new DAL.Models.Patient()
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