using System.Collections.Generic;

namespace PsychoTestDiploma.Models
{
    public class TestsResult
    {
        public string Id { get; set; }
        public List<Answer> Answers { get; set; }

        public class Answer
        {
            public int QuestionId { get; set; }
            public string ChosenAnswer { get; set; }
        }
    }
}