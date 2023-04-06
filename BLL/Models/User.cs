namespace BLL.Models
{
    public class User
    {
        public User(DAL.Models.User item)
        {
            Id = item.id;
            Login = item.login;
            Password = item.password;
            Role = item.role;
            Name = item.name;
        }
        public User() {}
        public string Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }

    }
}