using RepositoryLayer.Entities;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRepository
    {
        UserEntity? GetByEmail(string email);
        UserEntity? GetByGoogleId(string googleId); 
        void Add(UserEntity user);
        void Update(UserEntity user);
        UserEntity? GetById(int id);
        int? GetTokenVersion(int userId);
    }
}