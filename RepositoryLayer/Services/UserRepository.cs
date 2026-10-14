using Microsoft.EntityFrameworkCore;
using ModelLayer.Entities;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly FundooContext _db;

        public UserRepository(FundooContext db)
        {
            _db = db;
        }

        public UserEntity? GetByEmail(string email)
        {
            return _db.Users.FirstOrDefault(u => u.Email == email);
        }

        public void Add(UserEntity user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }

        public void Update(UserEntity user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }

        public UserEntity? GetById(int id)
        {
            return _db.Users.FirstOrDefault(u => u.Id == id);
        }

        public int? GetTokenVersion(int userId)
        {
            return _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => (int?)u.TokenVersion)
                .FirstOrDefault();
        }
    }
}
