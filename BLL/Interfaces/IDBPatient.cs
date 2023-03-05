using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IDBPatient
    {
        Task<IEnumerable<Patient>> GetPatients();
        Task<Patient> GetPatientById(string id);
        Task<Patient> GetPatientByToken(string token);
        Task<double> GetPatientsPagesCount();
        Task<IEnumerable<Patient>> GetPatientsWithCount(int pageNumber);
        Task<IEnumerable<Patient>> GetPatientsByName(string value);
        Task<double> GetPatientsByNamePagesCount(string value);
        Task<IEnumerable<Patient>> GetPatientsByNameWithCount(int pageNumber, string name);
        Task<Patient> GetPatientsResultsByTestId(string patientId, string testId);
        Task<string> CreatePatient(Patient p);
        Task UpdatePatient(Patient p);
        Task RemovePatient(string id);
    }
}