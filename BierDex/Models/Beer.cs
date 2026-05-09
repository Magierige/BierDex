using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BierDex.Models
{
    public class Beer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Barcode is verplicht")]
        public int barcode { get; set; }

        [Required(ErrorMessage = "Naam is verplicht")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn")]
        public string name { get; set; }

        [Required(ErrorMessage = "Type is verplicht")]
        public string type { get; set; }

        [Required(ErrorMessage = "Afbeelding pad is verplicht")]
        public string imagePath { get; set; }

        /// <summary>
        /// Validates: 0-100, optional 1 decimal, ends with %
        /// Examples: "5%", "5.5%", "100%", "0.1%"
        /// </summary>
        [Required(ErrorMessage = "Alcoholpercentage is verplicht")]
        [RegularExpression(@"^(100(\.0)?|([0-9]{1,2})(\.[0-9])?)%$",
            ErrorMessage = "Ongeldig formaat. Gebruik bijv. '5%', '5.5%' of '100%'")]
        public string abv { get; set; }

        // Navigation property
        public IdentityUser? user { get; set; }

        // Foreign Key
        [Required]
        public string userId { get; set; }

        // Constructor
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