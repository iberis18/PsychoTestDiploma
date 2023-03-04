using System;
using DAL.Interfaces;
using DAL.Models;

namespace DAL.Repositories
{
    public class DBUnitOfWork : IUnitOfWork
    {
        private Context db;
        private UserRepository userRepository;
        private PatientRepository patientRepository;
        private TestRepository testRepository;

        public DBUnitOfWork()
        {
            db = new Context();
        }
        public DBUnitOfWork(Context db)
        {
            this.db = db;
        }
        public IRepository<User> Users
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(db);
                return userRepository;
            }
        }

        public IRepository<Patient> Patients
        {
            get
            {
                if (patientRepository == null)
                    patientRepository = new PatientRepository(db);
                return patientRepository;
            }
        }

        public ITestRepository<Test> Tests
        {
            get
            {
                if (testRepository == null)
                    testRepository = new TestRepository(db);
                return testRepository;
            }
        }
    }
}