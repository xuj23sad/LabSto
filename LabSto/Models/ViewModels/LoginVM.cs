using System.ComponentModel.DataAnnotations;

namespace LabSto.Models.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "请输入用户名")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}