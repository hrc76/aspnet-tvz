using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Playlist.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB must contain only digits.")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 13)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG must contain only digits.")]
        public string JMBG { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
    }
}
