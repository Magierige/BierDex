using System.ComponentModel.DataAnnotations.Schema;

namespace BierDex.Models
{
    public class User
    {
        public int Id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public User() { }
        public User(int id, string email, string password, string username, int role)
        {
            Id = id;
            this.email = email;
            this.password = password;
            this.username = username;
            RoleId = role;
        }
    }
}
