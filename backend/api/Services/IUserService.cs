using backend.Models; 
namespace  backend.Services
{
    public interface IUserService
    {
        Task CreateAsync(User user);
        Task<User?> GetUserByEmail(string email); 
        Task<User?> GetUserByID(string id);
        Task<User?> UpdateUser(string id, User newuser);

        Task DeleteAsync(string id);
    }
}