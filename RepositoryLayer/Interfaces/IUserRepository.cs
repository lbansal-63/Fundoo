using ModelLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRepository
    {
        UserEntity? GetByEmail(string email);
        void Add(UserEntity user);
        void Update(UserEntity user);
    }
}


