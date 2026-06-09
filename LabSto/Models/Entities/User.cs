using System.ComponentModel.DataAnnotations;

namespace LabSto.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [StringLength(20)]
        public string Role { get; set; } = "普通用户";

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
