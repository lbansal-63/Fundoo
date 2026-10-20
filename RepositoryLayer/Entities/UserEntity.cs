using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int FailedLoginCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int TokenVersion { get; set; }
    }
}
