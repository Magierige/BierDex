using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


namespace BierDex.Models
{
    public class Beer
    {
        public int Id { get; set; }
        public int barcode { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string imagePath { get; set; }
        public string abv {  get; set; }
        public IdentityUser user { get; set; }
        public string userId { get; set; }
        public Beer(int id, int barcode, string name, string type, string imagePath, string abv, IdentityUser user)
        {
            this.Id = id;
            this.barcode = barcode;
            this.name = name;
            this.type = type;
            this.imagePath = imagePath;
            this.abv = abv;
            this.user = user;
            this.userId = user.Id;
        }
        public Beer() { }

    }
}
