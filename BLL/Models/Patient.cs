using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BLL.Models
{
    public class Patient
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public List<string> Tests { get; set; }
        public List<PatientResult>? Results { get; set; }

        public Patient() {}
        public Patient(DAL.Models.Patient item)
        {
            if (item != null)
            {
                Id = item.id;
                Name = item.name;
                Token = item.token;
                Tests = item.tests;
                Results = item.results.Select(i => new PatientResult(i)).ToList();
            }
        }
    }
}