using DataAccessLayer;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserWithAddressAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);

    }
}
