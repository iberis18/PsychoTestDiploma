using System.Collections.Generic;
using System.Linq;

namespace BLL.Models
{
    public class PatientResult
    {
        public string Test { get; set; }
        public string Date { get; set; }
        public List<Scale> Scales { get; set; }
        public string Comment { get; set; }

        public PatientResult()
        {
            Scales = new List<Scale>();
        }

        public PatientResult(DAL.Models.PatientResult item)
        {
            Test = item.test;
            Date = item.date;
            Comment = item.comment;
            Scales = item.scales.Select(i => new Scale(i)).ToList();
        }

        public class Scale
        {
            public string? IdTestScale { get; set; }
            public string? IdNormScale { get; set; }
            public string? Name { get; set; }
            public double? Scores { get; set; }
            public int? GradationNumber { get; set; }
            public string? Interpretation { get; set; }

            public Scale(){}
            public Scale(DAL.Models.PatientResult.Scale item)
            {
                IdTestScale = item.idTestScale;
                IdNormScale = item.idNormScale;
                Name = item.name;
                Scores = item.scores;
                GradationNumber = item.gradationNumber;
                Interpretation = item.interpretation;
            }
        }
    }
}