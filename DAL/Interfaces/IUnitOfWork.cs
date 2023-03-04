using System;
using DAL.Models;

namespace DAL.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<User> Users { get; }
        IRepository<Patient> Patients { get; }
        ITestRepository<Test> Tests { get; }
    }
}