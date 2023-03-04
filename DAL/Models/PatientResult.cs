using System.Collections.Generic;

namespace DAL.Models
{
    public class PatientResult
    {
        public string test { get; set; }
        public string date { get; set; }
        public List<Scale> scales { get; set; }
        public string comment { get; set; }

        public PatientResult()
        {
            scales = new List<Scale>();
        }
        public class Scale
        {
            public string? idTestScale { get; set; }
            public string? idNormScale { get; set; }
            public string? name { get; set; }
            public double? scores { get; set; }
            public int? gradationNumber { get; set; }
            public string? interpretation { get; set; }
        }
    }
}