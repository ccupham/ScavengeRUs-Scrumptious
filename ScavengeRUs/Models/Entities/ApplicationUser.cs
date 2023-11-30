using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ScavengeRUs.Models.Enums;

namespace ScavengeRUs.Models.Entities
{
    /// <summary>
    /// This is the object for a user. If you need to add a column to the user table this is where you do it. 
    /// The database table is built from this. If you change anything here add a new migration and update the database
    /// (in package manager console run "add-migration mig{xx}" "update-database"
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(50)]
        public string? FirstName { get; set; }
        [DisplayName("Last Name")]
        [StringLength(50)]
        [Required]
        public string? LastName { get; set; }
        public AccessCode? AccessCode { get; set; }
        [DisplayName("Registered Hunt")]
        public Hunt? Hunt{ get; set; }
        public ICollection<Location>? TasksCompleted { get; set; } = new List<Location>();
        [NotMapped]
        public ICollection<string> Roles { get; set; }
    = new List<string>();
        public Carriers Carrier { get; set; }
    }
}