namespace BLL.Models
{
    public class Test
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Instruction { get; set; }

        public Test(DAL.Models.Test item)
        {
            Id = item.id;
            Name = item.name;
            Title = item.title;
            Instruction = item.instruction;
        }
    }
}