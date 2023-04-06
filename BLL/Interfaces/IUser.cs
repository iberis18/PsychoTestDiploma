using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IUser
    {
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetIdentityUsers(string username, string password);
        Task<double> GetUsersPagesCount();
        Task<IEnumerable<User>> GetUsersWithCount(int pageNumber);
        Task<IEnumerable<User>> GetUsersByName(string value);
        Task<double> GetUsersByNamePagesCount(string value);
        Task<IEnumerable<User>> GetUsersByNameWithCount(int pageNumber, string name);
        Task<User> GetUserById(string id);
        Task CreateUser(User u);
        Task UpdateUser(User u);
        Task RemoveUser(string id);
    }
}